grammar Chat;

query
    :
    Select body
    ;

body
    :
    (query_seq | restrictions) ';'? (InWin number)?
    ;

query_seq
    :
    '(' query ')' ( ';' '(' query ')' )* 
    ;

restrictions
    :
    restriction (',' restriction)* Unr?
    ;

restriction
    :
    restriction And restriction
    | restriction Or restriction
    | '(' restriction ')'
    | Not restriction
    | condition
    ;

condition
    :
    HasWordOfDict '(' hdict ')'
    | HasTime '(' ')'
    | HasLocation '(' ')'
    | HasOrganization '(' ')'
    | HasURL '(' ')'
    | HasDate '(' ')'
    | HasQuestion '(' ')'
    | HasUserMentioned '(' huser ')'
    | ByUser '(' huser ')'
    ;

number : INTEGER;
hdict : STRING;
huser : STRING;

Select : 'SELECT' | 'select';
InWin  : 'INWIN'  | 'inwin' ;
Unr    : 'UNR'    | 'unr'   ;
Not    : 'NOT'    | 'not'   ;
And    : 'AND'    | 'and'   ;
Or     : 'OR'     | 'or'    ;

HasWordOfDict    : 'HASWORDOFDICT'    | 'haswordofdict'   ;
HasTime          : 'HASTIME'          | 'hastime'         ;
HasLocation      : 'HASLOCATION'      | 'haslocation'     ;
HasOrganization  : 'HASORGANIZATION'  | 'hasorganization' ;
HasURL           : 'HASURL'           | 'hasurl'          ;
HasDate          : 'HASDATE'          | 'hasdate'         ;
HasQuestion      : 'HASQUESTION'      | 'hasquestion'     ;
HasUserMentioned : 'HASUSERMENTIONED' | 'hasusermentioned';
ByUser           : 'BYUSER'           | 'byuser'          ;

INTEGER : DIGIT+;
STRING  : (LETTER | DIGIT)+;

WS: [ \n\r\t] -> skip;

fragment DIGIT : [0-9];
fragment LETTER : [a-zA-Z_];
