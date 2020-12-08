grammar Asm6502Grammar;

options { tokenVocab = Asm6502Lexer; }

prog
    : (line? EOL)+
    ;

line
    : comment
    | label
    | instruction
    | assemblerInstructionOrigin
    /*| assemblerInstructionInclude
    | assemblerInstructionIncludeBinary*/
    | defineConstant
    ;

instruction
    : label? opcode (addressMode)? comment?
    ;

addressMode
    : HASH constantExpression                                       # addressModeImmediate
    | REGISTER_A                                                    # addressModeAccumulator
    | LT constantExpression                                         # addressModeZeroPage
    | NOT constantExpression                                        # addressModeAbsolute
    | constantExpression                                            # addressModeRelativeOrAbsoluteOrZeroPage
    | LT constantExpression COMMA REGISTER_X                        # addressModeZeroPageWithX
    | NOT constantExpression COMMA REGISTER_X                       # addressModeAbsoluteWithX
    | constantExpression COMMA REGISTER_X                           # addressModeAbsoluteOrZeroPageWithX
    | LT constantExpression COMMA REGISTER_Y                        # addressModeZeroPageWithY
    | NOT constantExpression COMMA REGISTER_Y                       # addressModeAbsoluteWithY
    | constantExpression COMMA REGISTER_Y                           # addressModeAbsoluteOrZeroPageWithY
    | ROUND_OPEN constantExpression ROUND_CLOSE                     # addressModeIndirect
    | ROUND_OPEN constantExpression COMMA REGISTER_X ROUND_CLOSE    # addressModeIndirectWithX
    | ROUND_OPEN constantExpression ROUND_CLOSE COMMA REGISTER_Y    # addressModeIndirectWithY
    ;

assemblerInstructionOrigin
    : ORIGIN ROUND_OPEN constantExpression ROUND_CLOSE comment?
    ;

/*assemblerInstructionInclude
    : INCLUDE ROUND_OPEN string ROUND_CLOSE comment?
    ;*/

/*assemblerInstructionIncludeBinary
    : INCLUDE_BINARY ROUND_OPEN string ROUND_CLOSE comment?
    ;*/

defineConstant
    : CONST identifier EQUALS constantExpression comment?
    ;

constantExpression
    : ROUND_OPEN expression=constantExpression ROUND_CLOSE                          # constantExpressionBraces
    | left=constantExpression operation=POW right=constantExpression                # constantExpressionBinaryOperator
    | left=constantExpression operation=(MUL | DIV | MOD) right=constantExpression  # constantExpressionBinaryOperator
    | left=constantExpression operation=(ADD | SUB) right=constantExpression        # constantExpressionBinaryOperator
    | left=constantExpression operation=(LTLT | GTGT) right=constantExpression      # constantExpressionBinaryOperator
    | left=constantExpression operation=AMP right=constantExpression                # constantExpressionBinaryOperator
    | left=constantExpression operation=CARET right=constantExpression              # constantExpressionBinaryOperator
    | left=constantExpression operation=BAR right=constantExpression                # constantExpressionBinaryOperator
    | number                                                                        # constantExpressionNumber
    | identifierHiLo                                                                # constantExpressionIdentifierHiLo
    | identifier                                                                    # constantExpressionIdentifier
    ;

label
    : identifier COLON
    ;

number
    : HEX_NUMBER
    | BINARY_NUMBER
    | DECIMAL_NUMBER
    ;

identifierHiLo
    : identifier COLON HI
    | identifier COLON LO
    ;

identifier
    : IDENTIFIER
    | REGISTER_A /* The registers are added as valid identifiers so that we can produce errors  */
    | REGISTER_X /* during assemble time. If they are not included, a label such as "a:" will   */
    | REGISTER_Y /* not be considered a label. The same applies to opcodes and keywords.        */
    | opcode
    | keyword
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

keyword
    : ORIGIN
    | INCLUDE
    | INCLUDE_BINARY
    | CONST
    | HI
    | LO
    ;