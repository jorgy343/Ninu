lexer grammar Asm6502Lexer;

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
INCLUDE : I N C L U D E;
INCLUDE_BINARY : I N C L U D E '_' B I N A R Y;
CONST : C O N S T;

/*
    Operators and Stuff
*/
LTLT    : '<<';
GTGT    : '>>';
AMPAMP  : '&&';
BARBAR  : '||';

DOT     : '.';
COMMA   : ',';
COLON   : ':';
HASH    : '#';
EQUALS  : '=';
LT      : '<';
GT      : '>';
NOT     : '!';

POW     : '**';
MUL     : '*';
DIV     : '/';
MOD     : '%';
ADD     : '+';
SUB     : '-';
AMP     : '&';
CARET   : '^';
BAR     : '|';

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

BINARY_NUMBER
    : '%' [0-1]+
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