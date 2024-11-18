using System.Text;

namespace ORMini.Converters;

public class SnakeCaseConverter : ICaseConverter
{
    public string Convert(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }
        var sb = new StringBuilder();
        var isLastCharUpper = true;

        foreach (char c in value)
        {
            if (!char.IsUpper(c))
            {
                sb.Append(c);
                isLastCharUpper = false;
                continue;
            }

            if (isLastCharUpper)
            {
                sb.Append(char.ToLowerInvariant(c));
                continue;
            }

            sb.Append('_');
            sb.Append(char.ToLowerInvariant(c));
            isLastCharUpper = true;
        }

        return sb.ToString();
    }
}
