grammar Asm6502;

prog
    : (line? EOL)+
    ;

line
    : comment
    | label
    | instruction
    | assemblerInstructionOrigin
    | defineConstant
    ;

instruction
    : label? opcode (addressMode)? comment?
    ;

addressMode
    : HASH numberOrIdentifier                                       # addressModeImmediate
    | REGISTER_A                                                    # addressModeAccumulator
    | numberOrIdentifier                                            # addressModeAbsoluteRelativeZeroPage
    | numberOrIdentifier COMMA REGISTER_X                           # addressModeAbsoluteZeroPageWithX
    | numberOrIdentifier COMMA REGISTER_Y                           # addressModeAbsoluteZeroPageWithY
    | ROUND_OPEN numberOrIdentifier ROUND_CLOSE                     # addressModeIndirect
    | ROUND_OPEN numberOrIdentifier COMMA REGISTER_X ROUND_CLOSE    # addressModeIndirectWithX
    | ROUND_OPEN numberOrIdentifier ROUND_CLOSE COMMA REGISTER_Y    # addressModeIndirectWithY
    ;

assemblerInstructionOrigin
    : ORIGIN ROUND_OPEN constantExpression ROUND_CLOSE comment?
    ;

defineConstant
    : CONST identifier EQUALS constantExpression comment?
    ;

constantExpression
    : ROUND_OPEN expression=constantExpression ROUND_CLOSE                          # constantExpressionBraces
    | left=constantExpression operation=POW right=constantExpression                # constantExpressionOperator
    | left=constantExpression operation=(MUL | DIV | MOD) right=constantExpression  # constantExpressionOperator
    | left=constantExpression operation=(ADD | SUB) right=constantExpression        # constantExpressionOperator
    | numberOrIdentifier                                                            # constantExpressionNumberOrIdentifier
    ;

label
    : identifier COLON
    ;

numberOrIdentifier
    : number
    | identifier
    | identifierHiLo
    ;

number
    : HEX_NUMBER
    | DECIMAL_NUMBER
    ;

identifier
    : IDENTIFIER
    | REGISTER_A /* The registers are added as valid identifiers so that we can produce errors  */
    | REGISTER_X /* during assemble time. If they are not included, a label such as "a:" will   */
    | REGISTER_Y /* not be considered a label. The same applies to opcodes and keywords.        */
    | opcode
    | keywords
    ;

identifierHiLo
    : identifier COLON HI
    | identifier COLON LO
    ;

comment
    : COMMENT
    ;

opcode
    : AAX
    | ADC
    | AND
    | ASL
    | AXS
    | BCC
    | BCS
    | BEQ
    | BIT
    | BMI
    | BNE
    | BPL
    | BRK
    | BVC
    | BVS
    | CLC
    | CLD
    | CLI
    | CLV
    | CMP
    | CPX
    | CPY
    | DEC
    | DEX
    | DEY
    | DOP
    | EOR
    | INC
    | INX
    | INY
    | JMP
    | JSR
    | KIL
    | LAX
    | LDA
    | LDX
    | LDY
    | LSR
    | NOP
    | ORA
    | PHA
    | PHP
    | PLA
    | PLP
    | RLA
    | ROL
    | ROR
    | RRA
    | RTI
    | RTS
    | SBC
    | SEC
    | SED
    | SEI
    | SLO
    | STA
    | STX
    | STY
    | TAX
    | TAY
    | TOP
    | TSX
    | TXA
    | TXS
    | TYA;

keywords
    : ORIGIN
    | CONST
    | HI
    | LO
    ;

/* Alpha Fragments */
fragment A : 'a' | 'A';
fragment B : 'b' | 'B';
fragment C : 'c' | 'C';
fragment D : 'd' | 'D';
fragment E : 'e' | 'E';
fragment F : 'f' | 'F';
fragment G : 'g' | 'G';
fragment H : 'h' | 'H';
fragment I : 'i' | 'I';
fragment J : 'j' | 'J';
fragment K : 'k' | 'K';
fragment L : 'l' | 'L';
fragment M : 'm' | 'M';
fragment N : 'n' | 'N';
fragment O : 'o' | 'O';
fragment P : 'p' | 'P';
fragment Q : 'q' | 'Q';
fragment R : 'r' | 'R';
fragment S : 's' | 'S';
fragment T : 't' | 'T';
fragment U : 'u' | 'U';
fragment V : 'v' | 'V';
fragment W : 'w' | 'W';
fragment X : 'x' | 'X';
fragment Y : 'y' | 'Y';
fragment Z : 'z' | 'Z';

/*
    Opcodes
*/
AAX : A A X;
ADC : A D C;
AND : A N D;
ASL : A S L;
AXS : A X S;
BCC : B C C;
BCS : B C S;
BEQ : B E Q;
BIT : B I T;
BMI : B M I;
BNE : B N E;
BPL : B P L;
BRK : B R K;
BVC : B V C;
BVS : B V S;
CLC : C L C;
CLD : C L D;
CLI : C L I;
CLV : C L V;
CMP : C M P;
CPX : C P X;
CPY : C P Y;
DEC : D E C;
DEX : D E X;
DEY : D E Y;
DOP : D O P;
EOR : E O R;
INC : I N C;
INX : I N X;
INY : I N Y;
JMP : J M P;
JSR : J S R;
KIL : K I L;
LAX : L A X;
LDA : L D A;
LDX : L D X;
LDY : L D Y;
LSR : L S R;
NOP : N O P;
ORA : O R A;
PHA : P H A;
PHP : P H P;
PLA : P L A;
PLP : P L P;
RLA : R L A;
ROL : R O L;
ROR : R O R;
RRA : R R A;
RTI : R T I;
RTS : R T S;
SBC : S B C;
SEC : S E C;
SED : S E D;
SEI : S E I;
SLO : S L O;
STA : S T A;
STX : S T X;
STY : S T Y;
TAX : T A X;
TAY : T A Y;
TOP : T O P;
TSX : T S X;
TXA : T X A;
TXS : T X S;
TYA : T Y A;

/*
    Keywords
*/
ORIGIN : O R I G I N;
CONST : C O N S T;

/*
    Operators and Stuff
*/
DOT     : '.';
COMMA   : ',';
COLON   : ':';
HASH    : '#';
EQUALS  : '=';

POW : '^';
MUL : '*';
DIV : '/';
MOD : '%';
ADD : '+';
SUB : '-';

ROUND_OPEN  : '(';
ROUND_CLOSE : ')';
CURLY_OPEN  : '{';
CURLY_CLOSE : '}';

/*
    Everything Else
*/
REGISTER_A
    : A
    ;

REGISTER_X
    : X
    ;

REGISTER_Y
    : Y
    ;

HI
    : H I
    ;

LO
    : L O
    ;

IDENTIFIER
    : [_a-zA-Z] [_a-zA-Z0-9]*
    ;

HEX_NUMBER
    : '$' [0-9a-fA-F]+
    ;

DECIMAL_NUMBER
    : [0-9]+
    ;

COMMENT
   : ';' ~[\r\n]*
   ;

EOL
   : [\r\n]+
   ;

WS
   : [ \t] -> skip
   ;