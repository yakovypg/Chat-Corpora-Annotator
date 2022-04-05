namespace KeyPhraseExtractionAlgorithm

module KeyPhraseExtractor =
    open Edu.Stanford.Nlp.Pipeline

    let getKeyPhrases (tree:ParseTree) =
        let isNPwithNNx (node:ParseTree) =
            if (node.Value <> "NP") then false
            else node.Child
                |> Seq.cast<ParseTree>
                |> Seq.exists (fun x->
                   let y = x.Value
                   y = "NN" || y = "NNS" || y = "NNP" || y = "NNPS")
        let rec foldTree acc (node:ParseTree) =
            let acc =
                if (node.Child = null) then acc
                else node.Child
                    |> Seq.cast<ParseTree>
                    |> Seq.fold
                       (fun state x -> foldTree state x)
                       acc
            if isNPwithNNx node 
              then node :: acc
              else acc
        foldTree [] tree
