#pragma warning disable SA1204, SA1009, CS1591, SA1600, SA1200, SA1633
using System.Collections.Generic;
using System.Collections.Immutable;
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
    public class EventDataParserGenerator : ISourceGenerator
    {
        private readonly string[] usings = new string[]
        {
            "System",
            "System.Text.Json",
            "System.Text.Json.Serialization",
            "System.Threading",
            "System.Threading.Tasks",
            "Brighid.Discord.Events",
            "Brighid.Discord.Messages",
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
        };

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var events = from tree in context.Compilation.SyntaxTrees.AsParallel()
                         let semanticModel = context.Compilation.GetSemanticModel(tree)

                         from node in tree.GetRoot().DescendantNodesAndSelf().OfType<StructDeclarationSyntax>()
                         from attr in semanticModel.GetDeclaredSymbol(node)?.GetAttributes() ?? ImmutableArray.Create<AttributeData>()
                         where attr.AttributeClass != null && TypeUtils.IsSymbolEqualToType(attr.AttributeClass, typeof(GatewayEventAttribute))
                         let eventAttribute = TypeUtils.CreateAttribute<GatewayEventAttribute>(attr)
                         select (node, eventAttribute);

            var members = new MemberDeclarationSyntax[] { GenerateParseMember(events) };
            var classDeclaration = ClassDeclaration("GeneratedMessageParser")
                .WithModifiers(TokenList(Token(PublicKeyword), Token(PartialKeyword)))
                .WithMembers(List(members));

            var namespaceDeclaration = NamespaceDeclaration(ParseName("Brighid.Discord.Messages"))
                .WithMembers(List(new MemberDeclarationSyntax[] { classDeclaration }));

            var codes = ignoredCodes.Select(code => ParseExpression(code));
            var ignoreWarningsTrivia = Trivia(PragmaWarningDirectiveTrivia(Token(DisableKeyword), SeparatedList(codes), true));
            var usings = List(this.usings.Select(@using => UsingDirective(ParseName(@using))));
            var compilationUnit = CompilationUnit(List<ExternAliasDirectiveSyntax>(), usings, List<AttributeListSyntax>(), List(new MemberDeclarationSyntax[] { namespaceDeclaration }))
                .WithLeadingTrivia(TriviaList(ignoreWarningsTrivia));

            var generatedBody = compilationUnit.NormalizeWhitespace().GetText(Encoding.UTF8);
            context.AddSource("GeneratedMessageParser", generatedBody + "\n");
        }

        public static MemberDeclarationSyntax GenerateParseMember(IEnumerable<(StructDeclarationSyntax Node, GatewayEventAttribute Attribute)> events)
        {
            var identifier = Identifier("message");
            var parameters = SeparatedList(new ParameterSyntax[]
            {
                Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName("GatewayMessageWithoutData"), identifier, null),
                Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName("JsonSerializerOptions"), Identifier("options"), null),
            });

            IEnumerable<StatementSyntax> GenerateBody()
            {
                var switchArms = (from ev in events
                                  let codePattern = Subpattern(ConstantPattern(ParseExpression($"GatewayOpCode.{ev.Attribute.OpCode}")))
                                  let namePattern = ev.Attribute.EventName != null
                                                      ? Subpattern(ConstantPattern(ParseExpression($"\"{ev.Attribute.EventName}\"")))
                                                      : Subpattern(ConstantPattern(ParseExpression("null")))

                                  let subpatterns = SeparatedList(new SubpatternSyntax[] { codePattern, namePattern })
                                  let positionalPattern = PositionalPatternClause(subpatterns)
                                  let recursivePattern = RecursivePattern(ParseTypeName("GatewayMessageWithoutData"), positionalPattern, null, null)
                                  select SwitchExpressionArm(recursivePattern, ParseExpression($"message.WithData(JsonSerializer.Deserialize<{ev.Node.Identifier.Value}>(text, options))"))).ToList();

                var discardPattern = SwitchExpressionArm(DiscardPattern(), ParseExpression("message.WithData(null)"));
                switchArms.Add(discardPattern);

                var switchExpression = SwitchExpression(IdentifierName(identifier), Token(SwitchKeyword), Token(OpenBraceToken), SeparatedList(switchArms), Token(CloseBraceToken));

                yield return ParseStatement("message.ExtensionData.TryGetValue(\"d\", out var data);");
                yield return ParseStatement("var text = (data.ValueKind == JsonValueKind.Undefined) ? \"{}\" : data.GetRawText();");
                yield return ReturnStatement(switchExpression);
            }

            return MethodDeclaration(ParseTypeName($"GatewayMessage"), "ParseEventData")
                .WithModifiers(TokenList(Token(PublicKeyword)))
                .WithParameterList(ParameterList(parameters))
                .WithBody(Block(GenerateBody()));
        }
    }
}
