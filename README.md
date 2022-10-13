# Chat Corpora Annotator
This is a tool for creating and viewing chat-based corpora with a custom tagging model and a query language.

## Guide (under development)
CCA is best suited for viewing and tagging table data that contains three fields - 
* a *date* (in any date format), 
* a *username* (short string), 
* and a *text* field.

So far, these fields are necessary in order to load the data into the tool. 
### Basics


#### Step 1. Indexing a new file
Currently, CCA supports CSV (or TSV) files with all common delimiters. 
1. In order to index a new dataset, click **File -> Index new file**. 
2. Select a file, and in the following dialogue select the correct delimiter.
3. Select which columns of your file you would like to upload into the tool. 
4. Select which columns correspond to the **date**, **username**, and **text** fields.

#### Step 2. Creating a custom tagset
Currently, CCA supports only annotation schemes that are intended for tagging *subsets* of messages. You cannot assign tags to parts of messages.

In order to tag subsets of messages, you need to create a tagset. To do that,
1. Click Auxiliary -> Tagset editor.
2. Enter the name of the new tagset using the text field and click Add tagset.
3. Add tags to the tagset via the text field and the Add tag button. The color field next to the text field will allow you to select the color that the tagged messages will be highlighted with.
4. Use the *Set* button on top in order to set a tagset for the project. 
  * You can change the project tagset later, but this will delete previously tagged messages.
  * The selected tagset will be displayed in the main window.

#### Step 3. Tagging
In order to tag subsets of messages, 
1. Select messages with click+drag
2. Click on the necessary tag in the Tagset
3. Click Add tag. This will add a tag and an index to the subset.
4. You can use the Remove tag button to remove tags from any message

### Exploration
#### Basic exploration
Use the Main window in order to access these tools.
##### Search
Use the Live filtering menu in order to filter messages by the text field, or the Query menu in order to run full-text Lucene queries against the text field. You can also add user and date restraints. 

##### Concordance
Concordance is very similar to NLTK's `concordance()` function: it displays the results of a query with character-level context.

##### N-Gram Search
This will build and save to disk a B+-Tree based index of all n \in [2,5] - grams of the text and will provide search capabilities for it. If your dataset is large (around 100k rows or more) it will be quite slow. 

##### Keyword Analysis
This implements an algorithm for automatic keyword extraction. In order to use it, you will need to connect a CoreNLP instance.

##### Visualize
This menu provides two simple visualizations of the data - a plot and and a heatmap of the quantity of messages by date. 

#### Advanced exploration
In order to use the functionality described in this section, CCA will create a local CoreNLP instance first and pre-extract necessary linguistic features.
1. Click *Extractor*.
2. Select the path in which a functional CoreNLP distribution can be found. This will require Java. (See CoreNLP installation instructions here).
3. Download the required models
4. If all indicators are green, click Extract.

##### Suggester
Suggester is an interface to Macther - our custom query extraction language.
To access Suggester, click Auxiliary -> Suggester.

Matcher is based on Boolean retrieval model and pattern matching, and its main purpose is to select groups of messages wrapped in suggestions: as in, it allows the user to select `n` messages, which are then wrapped in their context in the dataset. A detailed explanation can be found in the following papers:

```
@inproceedings{smirnova-etal-2021-situation,
    title = "Situation-Based Multiparticipant Chat Summarization: a Concept, an Exploration-Annotation Tool and an Example Collection",
    author = "Smirnova, Anna  and
      Slobodkin, Evgeniy  and
      Chernishev, George",
    booktitle = "Proceedings of the 59th Annual Meeting of the Association for Computational Linguistics and the 11th International Joint Conference on Natural Language Processing: Student Research Workshop",
    month = aug,
    year = "2021",
    address = "Online",
    publisher = "Association for Computational Linguistics",
    url = "https://aclanthology.org/2021.acl-srw.14",
    doi = "10.18653/v1/2021.acl-srw.14",
    pages = "127--137",
}
```

and

```
Query Processing and Optimization for a Custom Retrieval Language Yakov Kuzin, Anna Smirnova, Evgeniy Slobodkin and George Chernishev
Pan-DL @ COLING 2022
```

The currently available operators are the following:
* `select` - Start a query or a subquery.
* `(`, `)`: Subqueries have to be surrounded by parentheses. Can also be used in Boolean retreival.
* `;`: Separator for subqueries.
* `and`, `or`, `not`: Boolean operators.
* `,`: Group separator
* `inwin n`: Contstraint that limits the window between matches
* `haswordofdict(dict)` matches messages that contain any word from a pre-specified named list;
* `hasdate()`, `hastime()`, `haslocation()`,`hasorganization()`, `hasurl()` match messages that contain tokens with the respective Named Entity tags
* `hasusermentioned(user)` matches messages that contain a username mention in the text field;
* `byuser(user)` matches messages that contain a specified username in the user field;
* `hasquestion()` matches messages that contain at least one question-like sentence.
    
The matching operators can be combined with Boolean copulas.

The `development/benchmarks/SuggesterBenchmark/` contains the benchmark code and several query examples.


