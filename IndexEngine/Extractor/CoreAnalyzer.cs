using edu.stanford.nlp.ling;
using edu.stanford.nlp.pipeline;
using edu.stanford.nlp.trees;
using edu.stanford.nlp.util;
using IndexEngine.Paths;
using java.util;

namespace IndexEngine.NLP
{
    public class CoreAnalyzer
    {


        public Tree ConstituencyParse { get; }
        private Tree constituencyParse;
        public object[] treeArray;


        public StanfordCoreNLP SimplePipeline()
        {
            Properties props = new Properties();

            props.setProperty("annotators", "tokenize,ssplit,pos,lemma,ner,parse");

            props.setProperty("pos.model", ToolInfo.POSpath);
            props.setProperty("ner.model", ToolInfo.NERpath);
            props.setProperty("parse.model", ToolInfo.SRparserpath);

            props.setProperty("ner.useSUTime", "true");
            props.setProperty("ner.applyFineGrained", "false");

            props.setProperty("sutime.binders", "0");

            props.setProperty("sutime.rules", ToolInfo.sutimeRules);
            props.setProperty("sutime.markTimeRanges", "true");
            props.setProperty("sutime.includeNested", "true");
            StanfordCoreNLP pipeline = new StanfordCoreNLP(props);
            return pipeline;
        }

        public void CreateParseTree(CoreDocument coredoc)
        {
            if (coredoc != null)
            {
                ArrayList sents = (ArrayList)coredoc.annotation().get(typeof(CoreAnnotations.SentencesAnnotation));
                for (int i = 0; i < sents.size(); i++)
                {
                    CoreMap sentence = (CoreMap)sents.get(i);

                    this.constituencyParse = (Tree)sentence.get(typeof(TreeCoreAnnotations.TreeAnnotation));

                    Set treeConstituents = (Set)constituencyParse.constituents(new LabeledScoredConstituentFactory());
                    treeArray = treeConstituents.toArray();

                }
            }
        }

        public Tree GetParseTree(CoreMap sent)
        {


            return (Tree)sent.get(typeof(TreeCoreAnnotations.TreeAnnotation));


        }

        public ArrayList GetSents(CoreDocument coredoc)
        {
            ArrayList sents = new ArrayList();
            if (coredoc != null)
            {
                sents = (ArrayList)coredoc.annotation().get(typeof(CoreAnnotations.SentencesAnnotation));
                return sents;
            }
            else { return null; }

        }


    }
}
