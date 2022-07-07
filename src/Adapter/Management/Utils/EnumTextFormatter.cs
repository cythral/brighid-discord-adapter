using System;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Formatters;

namespace Brighid.Discord.Adapter.Management
{
    /// <inheritdoc />
    public class EnumTextFormatter : TextInputFormatter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumTextFormatter" /> class.
        /// </summary>
        public EnumTextFormatter()
        {
            SupportedMediaTypes.Add("text/plain");
            SupportedEncodings.Add(UTF8EncodingWithoutBOM);
            SupportedEncodings.Add(UTF16EncodingLittleEndian);
        }

        /// <inheritdoc />
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            using var streamReader = context.ReaderFactory(context.HttpContext.Request.Body, encoding);
            var data = await streamReader.ReadToEndAsync();
            var result = Enum.Parse(context.ModelType, data);
            return InputFormatterResult.Success(result);
        }

        /// <inheritdoc />
        protected override bool CanReadType(Type type)
        {
            return type.IsEnum;
        }
    }
}
