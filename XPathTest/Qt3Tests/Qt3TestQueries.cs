namespace XPathTest.Qt3Tests;

public static class Qt3TestQueries
{
    public const string AllTestNameQuery =
        @"/test-set/@name || /test-set/description!(if (string()) then ""~"" || . else """")";

    public const string AllTestsQuery = "/test-set/test-case";
//     public const string AllTestsQuery = @"
// /test-set/test-case[
//     not(exists((./dependency | ../dependency)[@type=""xml-version"" and @value=""1.1""])) and not(
//      (./dependency | ../dependency)/@value/tokenize(.) = (
//        ""XQ10"",
//        ""XQ20"",
//        ""XQ30"",
//        ""schemaValidation"",
//        ""schemaImport"",
//        (:""staticTyping"",:)
//        (:""serialization"",:)
//        ""infoset-dtd"",
//        (:""xpath-1.0-compatibility"",:)
//        ""namespace-axis"",
//        (:""moduleImport"",:)
//        ""schema-location-hint"",
//        (:""collection-stability"",:)
//        ""directory-as-collation-uri"",
//        (:""fn-transform-XSLT"",:)
//        (:""fn-transform-XSLT30"",:)
//        (:""fn-format-integer-CLDR"",:)
//        (:""non-empty-sequence-collection"",:)
//        ""non-unicode-codepoint-collation"",
//        ""simple-uca-fallback"",
//        ""advanced-uca-fallback""))]
// ";
}