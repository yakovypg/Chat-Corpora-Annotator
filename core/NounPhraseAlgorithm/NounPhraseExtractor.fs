namespace NounPhraseAlgorithm

module NPExtractor = 

    open edu.stanford.nlp.trees
    open java.util
 
    let toSeq (iter:Iterator) =
        let rec loop (x:Iterator) = 
            seq { 
                if x.hasNext() then yield x.next()
                if x.hasNext() then 
                    yield! (loop x)
                }
        loop iter 
     
    let getKeyPhrases (tree:Tree) = 
        let isNPwithNNx (node:Tree)= 
            if (node.label().value() <> "NP") then false
            else node.getChildrenAsList().iterator()
                 |> toSeq 
                 |> Seq.cast<Tree>
                 |> Seq.exists (fun x-> 
                    let y = x.label().value()
                    y= "NN" || y = "NNS" || y = "NNP" || y = "NNPS")
        let rec foldTree acc (node:Tree) = 
            let acc = 
                if (node.isLeaf()) then acc
                else node.getChildrenAsList().iterator()
                     |> toSeq 
                     |> Seq.cast<Tree>
                     |> Seq.fold 
                        (fun state x -> foldTree state x)
                        acc
            if isNPwithNNx node 
              then node :: acc
              else acc
        foldTree [] tree
