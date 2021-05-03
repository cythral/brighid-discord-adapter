#pragma warning disable SA1204, SA1009, CS1591, SA1600, SA1200, SA1633
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Brighid.Discord.Generators
{
    [Generator]
    public class EventRouterGenerator : ISourceGenerator
    {
        private readonly string[] usings = new string[]
        {
            "System",
            "System.Text.Json",
            "System.Text.Json.Serialization",
            "System.Threading",
            "System.Threading.Tasks",
            "Brighid.Discord.GatewayAdapter.Events",
            "Brighid.Discord.GatewayAdapter.Messages",
            "Microsoft.Extensions.DependencyInjection",
        };

        private readonly string[] ignoredCodes = new string[]
        {
            "CS1591",
            "SA1601",
            "SA1413",
            "SA1633",
            "SA1600",
            "SA1101",
            "SA1015",
            "SA1012",
            "SA1009",
            "SA1013",
            "SA1025",
            "SA1119",
        };

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var events = from tree in context.Compilation.SyntaxTrees.AsParallel()
                         let semanticModel = context.Compilation.GetSemanticModel(tree)

                         from node in tree.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>()
                         from attr in semanticModel.GetDeclaredSymbol(node)?.GetAttributes() ?? ImmutableArray.Create<AttributeData>()
                         where attr.AttributeClass != null && TypeUtils.IsSymbolEqualToType(attr.AttributeClass, typeof(EventControllerAttribute))
                         select (node, attr);

            File.WriteAllText("/tmp/test", "Number of event controllers: " + events.Count() + "\n");
            var members = new MemberDeclarationSyntax[] { GenerateParseMember(events) };
            var classDeclaration = ClassDeclaration("GeneratedEventRouter")
                .WithModifiers(TokenList(Token(PublicKeyword), Token(PartialKeyword)))
                .WithMembers(List(members));

            var namespaceDeclaration = NamespaceDeclaration(ParseName("Brighid.Discord.GatewayAdapter.Events"))
                .WithMembers(List(new MemberDeclarationSyntax[] { classDeclaration }));

            var codes = ignoredCodes.Select(code => ParseExpression(code));
            var ignoreWarningsTrivia = Trivia(PragmaWarningDirectiveTrivia(Token(DisableKeyword), SeparatedList(codes), true));
            var usings = List(this.usings.Select(@using => UsingDirective(ParseName(@using))));
            var compilationUnit = CompilationUnit(List<ExternAliasDirectiveSyntax>(), usings, List<AttributeListSyntax>(), List(new MemberDeclarationSyntax[] { namespaceDeclaration }))
                .WithLeadingTrivia(TriviaList(ignoreWarningsTrivia));

            var generatedBody = compilationUnit.NormalizeWhitespace().GetText(Encoding.UTF8);
            context.AddSource("GeneratedEventRouter", generatedBody + "\n");
        }

        public static MemberDeclarationSyntax GenerateParseMember(IEnumerable<(ClassDeclarationSyntax Node, AttributeData Attribute)> controllers)
        {
            var identifier = Identifier("@event");
            var parameters = SeparatedList(new ParameterSyntax[]
            {
                Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName("IGatewayEvent"), identifier, null),
                Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName("CancellationToken"), Identifier("cancellationToken"), null),
            });

            IEnumerable<StatementSyntax> GenerateBody()
            {
                var switchArms = (from ev in controllers
                                  let typeSymbol = (INamedTypeSymbol)ev.Attribute.ConstructorArguments[0].Value!
                                  let designatedName = typeSymbol.Name.ToLower() + "Instance"
                                  let pattern = DeclarationPattern(ParseName(typeSymbol.Name), SingleVariableDesignation(Identifier(designatedName)))
                                  select SwitchExpressionArm(pattern, ParseExpression($"scope.ServiceProvider.GetRequiredService<{ev.Node.Identifier.Text}>().Handle({designatedName}, cancellationToken)"))).ToList();

                var separators = (from ev in controllers
                                  select Token(CommaToken).WithLeadingTrivia(ParseLeadingTrivia("\r\n"))).ToList();

                var discardPattern = SwitchExpressionArm(DiscardPattern(), ParseExpression("throw new Exception()"));
                switchArms.Add(discardPattern);

                var switchExpression = SwitchExpression(IdentifierName(identifier), Token(SwitchKeyword), Token(OpenBraceToken), SeparatedList(switchArms, separators), Token(CloseBraceToken));
                var parenthesizedExpression = ParenthesizedExpression(switchExpression);
                var awaitExpression = AwaitExpression(parenthesizedExpression);

                yield return ParseStatement("cancellationToken.ThrowIfCancellationRequested();");
                yield return ParseStatement("using var scope = serviceProvider.CreateScope();");
                yield return ExpressionStatement(awaitExpression);
            }

            return MethodDeclaration(ParseTypeName($"Task"), "Route")
                .WithModifiers(TokenList(Token(PublicKeyword), Token(AsyncKeyword)))
                .WithParameterList(ParameterList(parameters))
                .WithBody(Block(GenerateBody()));
        }
    }
}
