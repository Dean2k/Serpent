namespace Serpent
{
    public static class extension
    {
        public static string replaceThis(this string input, string replaceToBlank)
        {
            try
            {
                if (input != null)
                {
                    return input.Replace("'", "");
                }
                else
                {
                    return "None";
                }
            }
            catch
            {
                return "None";
            }
        }
    }
}
