using System.Collections.Generic;
using CommandLine;

namespace PostMediumGitHubAction
{
    public class Settings
    { 
        [Option('e', "title", Required = true, HelpText = "The post's title.")]
        public string Title { get; set; }

        [Option('a', "tags", Required = true, HelpText = "The post’s tags.", Separator = ',')]
        public IEnumerable<string> Tags { get; set; }

        [Option('u', "canonical-url", Required = false, HelpText = "The canonical URL of the post. " +
                                                                 "If canonicalUrl was not specified in the creation of the post, this field will not be present.")]
        public string CanonicalUrl { get; set; }

        [Option('s', "publish-status", Required = false, HelpText = "The publish status of the post. Valid values are: Draft, Public or Unlisted")]
        public string PublishStatus { get; set; } = "draft";


        [Option('l', "license", Required = false, HelpText = "The license of the post. Valid values are all-rights-reserved, " +
                                                             "cc-40-by, cc-40-by-sa, cc-40-by-nd,  cc-40-by-nc, cc-40-by-nc-nd, cc-40-by-nc-sa, cc-40-zero, public-domain.")]
        public string License { get; set; } = "all-rights-reserved";

        [Option('p', "publication-id", Required = false, HelpText = "The id of the publication the post is being created under. If you do not know the Id, use PublicationName")]
        public string PublicationId { get; set; }

        [Option('n', "publication-name", Required = false, HelpText = "The name of the publication the post is being created under. Either PublicationName of PublicationId should be set.")]
        public string PublicationName { get; set; }

        [Option('f', "notify-followers", Required = false, HelpText = "Whether to notify followers that the user has published.")]
        public bool NotifyFollowers { get; set; } = false;

        [Option('o', "content-format", Required = true, HelpText = "The format of the \"content\" field. There are two valid values, \"html\", and \"markdown\"")]
        public string ContentFormat { get; set; }

        [Option('c', "content", Required = false, HelpText = "The body of the post, in a valid, semantic, HTML fragment, or Markdown. " +
                                                             "Further markups may be supported in the future. " +
                                                             "If you want your title to appear on the post page, you must also include it as part of the post content.")]
        public string Content { get; set; }

        [Option('i', "file", Required = false, HelpText = "Path to the file that should be read and used as content. If this is set, this will overrule the Content property.")]
        public string File { get; set; }

        [Option('t', "integration-token", Required = true, HelpText = "The token that should be used to authenticate in the API. Token can be retrieved at https://medium.com/me/settings under \"Integration Token\"")]
        public string IntegrationToken { get; set; }
    }
}