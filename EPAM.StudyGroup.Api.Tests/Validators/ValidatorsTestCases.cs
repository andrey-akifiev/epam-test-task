namespace EPAM.StudyGroup.Api.Tests.Validators
{
    public static class ValidatorsTestCases
    {
        public static IEnumerable<object[]> ValidPasswordCases()
        {
            string supportedSpecialCharacters = "# !()-.?[]_`~@#$^&*+={}\"'%/\\;:<>";
            foreach (var specialCharacter in supportedSpecialCharacters.ToCharArray())
            {
                yield return new object[] { $"Aa{specialCharacter}12345" };
            }
        }

        public static IEnumerable<object[]> ValidEmailCases()
        {
            string[] validEmails = new string[]
            {
                "Thomas.Müller@epg-fnancials.com",
                "T.Müller@epg-fnancials.com",
                "email@example.com",
                "firstname.lastname@example.com",
                "email@subdomain.example.com",
                "firstname+lastname@example.com",
                "email@123.123.123.123",
                "email@[123.123.123.123]",
                "\"email\"@example.com",
                "1234567890@example.com",
                "email@example-one.com",
                "_______@example.com",
                "email@example.name",
                "email@example.museum",
                "email@example.co.jp",
                "firstname-lastname@example.com",
                "a.user-name@example.com",
                "a.use@example",
                "dot.dot.tochka@example",
                "....tochka@example",
            };

            foreach (var email in validEmails)
            {
                yield return new object[] { email };
            }
        }

        public static IEnumerable<object[]> InvalidEmailCases()
        {
            string[] validEmails = new string[]
            {
                "plainaddress",
                "@example.com",
                "email.example.com"
            };

            foreach (var email in validEmails)
            {
                yield return new object[] { email };
            }
        }

        public static IEnumerable<object[]> NullAndEmptyStringVariations()
        {
            var variations = new string[] { null, "", " ", "\n", "\t" };

            foreach (var variation in variations)
            {
                yield return new object[] { variation };
            }
        }

        public static IEnumerable<object[]> EmptyStringVariations()
        {
            var variations = new string[] { "", " ", "\n", "\t" };

            foreach (var variation in variations)
            {
                yield return new object[] { variation };
            }
        }
    }
}