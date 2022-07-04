namespace XPathTest;

public class Qt3TestQueries
{
    public const string AllTestNameQuery =
        @"/test-set/@name || /test-set/description!(if (string()) then ""~"" || . else """")";

    public const string AllTestsQuery = @"
/test-set/test-case[
  let $dependencies := (./dependency | ../dependency)
  return not(exists($dependencies[@type=""xml-version"" and @value=""1.1""])) and not(
     $dependencies/@value/tokenize(.) = (
       ""XQ10"",
       ""XQ20"",
       ""XQ30"",
       ""schemaValidation"",
       ""schemaImport"",
       (:""staticTyping"",:)
       (:""serialization"",:)
       ""infoset-dtd"",
       (:""xpath-1.0-compatibility"",:)
       ""namespace-axis"",
       (:""moduleImport"",:)
       ""schema-location-hint"",
       (:""collection-stability"",:)
       ""directory-as-collation-uri"",
       (:""fn-transform-XSLT"",:)
       (:""fn-transform-XSLT30"",:)
       (:""fn-format-integer-CLDR"",:)
       (:""non-empty-sequence-collection"",:)
       ""non-unicode-codepoint-collation"",
       ""simple-uca-fallback"",
       ""advanced-uca-fallback""))]
";
}