lexer grammar EplLexer;
tokens {
	Separator,
	Public,
	Optional,
	Array,
	ByRef,
	Static,
	StringItem
}
fragment SpaceChar: [ \t];
fragment Digit: [0-9];
Class: '.程序集' SpaceChar* -> mode(ClassAttr1);
Method: '.子程序' SpaceChar* -> mode(MethodAttr1);
Arg: '.参数' SpaceChar* -> mode(ArgAttr1);
Local: '.局部变量' SpaceChar* -> mode(VariableAttr1);
ClassVariable: '.程序集变量' SpaceChar* -> mode(VariableAttr1);
Global: '.全局变量' SpaceChar* -> mode(VariableAttr1);
Struct: '.数据类型' SpaceChar* -> mode(StructAttr1);
StructMember: '.成员' SpaceChar* -> mode(VariableAttr1);
DllDeclare: '.DLL命令' SpaceChar* -> mode(DllDeclareAttr1);
Constant: '.常量' SpaceChar* -> mode(ConstantAttr1);
ImageResource: '.图片' SpaceChar* -> mode(ResourceAttr1);
SoundResource: '.声音' SpaceChar* -> mode(ResourceAttr1);
TextResource: '.长文本' SpaceChar* -> mode(ResourceAttr1);
IfTrue: '.如果真' -> mode(NormalCode);
EndIfTrue: '.如果真结束' -> mode(NormalCode);
If: '.如果' -> mode(NormalCode);
Else: '.否则' -> mode(NormalCode);
EndIf: '.如果结束' -> mode(NormalCode);
For: '.变量循环首' -> mode(NormalCode);
EndFor: '.变量循环尾' -> mode(NormalCode);
Counter: '.计次循环首' -> mode(NormalCode);
EndCounter: '.计次循环尾' -> mode(NormalCode);
While: '.判断循环首' -> mode(NormalCode);
EndWhile: '.判断循环尾' -> mode(NormalCode);
DoWhile: '.循环判断首' -> mode(NormalCode);
EndDoWhile: '.循环判断尾' -> mode(NormalCode);
BeginWhen: '.判断开始' -> mode(NormalCode);
WhenCase: '.判断' -> mode(NormalCode);
WhenElse: '.默认' -> mode(NormalCode);
EndWhen: '.判断结束' -> mode(NormalCode);
fragment StringItem: (["] ~["\r\n]* ["] | ~[,"\r\n]+);
StringLiteral: ["“”] ~["“”\r\n]* ["“”] -> mode(NormalCode);
Newline: ( '\r' '\n'? | '\n');
CommentPrefix: '\'' SpaceChar? -> skip, mode(InsideComment);
NumberLiteral: (Plus | Minus)? (
		Digit+ (Dot Digit*)?
		| Dot Digit+
	) ([eE] (Plus | Minus)? Digit+)? -> mode(NormalCode);
DateTimeLiteral:
	LSquareBracket Digit+ '年' Digit+ '月' Digit+ '日' (
		Digit+ '时' Digit+ '分' Digit+ '秒'
	)? RSquareBracket -> mode(NormalCode);
Dot: ('.' | '。') -> mode(NormalCode);
And: ('&&' | 'And' | Space '且' Space) -> mode(NormalCode);
Or: ('||' | 'Or' | Space '或' Space) -> mode(NormalCode);
Comma: [,，] -> mode(NormalCode);
Assign: ('=' | '＝') -> mode(NormalCode), pushMode(NormalCodeInside);
Equal: '==' -> mode(NormalCode);
NotEqual: ('!=' | '<>' | '≠') -> mode(NormalCode);
Less: ('<' | '＜') -> mode(NormalCode);
Greater: ('>' | '＞') -> mode(NormalCode);
LessOrEqual: ('<=' | '≤') -> mode(NormalCode);
GreaterOrEqual: ('>=' | '≥') -> mode(NormalCode);
ApproximatelyEqual: ('?=' | '≈') -> mode(NormalCode);
Minus: ('-' | '－') -> mode(NormalCode);
Plus: ('+' | '＋') -> mode(NormalCode);
Mul: ('*' | '×') -> mode(NormalCode);
Div: ('/' | '÷') -> mode(NormalCode);
IDiv: ('\\' | '＼') -> mode(NormalCode);
Mod: ('%' | Space 'Mod' Space | '％') -> mode(NormalCode);
Hash: '#' -> mode(NormalCode);
LParen: ('(' | '（') -> mode(NormalCode), pushMode(NormalCodeInside);
RParen: (')' | '）') -> mode(NormalCode);
LSquareBracket:
	'[' -> mode(NormalCode), pushMode(NormalCodeInside);
RSquareBracket: ']' -> mode(NormalCode);
LBrace: '{' -> mode(NormalCode), pushMode(NormalCodeInside);
RBrace: '}' -> mode(NormalCode);
Identifier: ([_\p{ID_Start}] [_\p{ID_Continue}]*) -> mode(NormalCode);
AddrOf: '&' -> mode(NormalCode);
Space: SpaceChar+ -> skip;
Unknown: .;

mode NormalCode;
StringLiteralNormal: StringLiteral -> type(StringLiteral);
NewlineNormal: Newline -> type(Newline), mode(DEFAULT_MODE);
CommentPrefixNormal:
	CommentPrefix -> skip, mode(InsideComment);
NumberLiteralNormal: NumberLiteral -> type(NumberLiteral);
DateTimeLiteralNormal: DateTimeLiteral -> type(DateTimeLiteral);
DotNormal: Dot -> type(Dot);
AndNormal: And -> type(And);
OrNormal: Or -> type(Or);
CommaNormal: Comma -> type(Comma);
AssignNormal:
	Assign -> type(Assign), pushMode(NormalCodeInside);
EqualNormal: Equal -> type(Equal);
NotEqualNormal: NotEqual -> type(NotEqual);
LessNormal: Less -> type(Less);
GreaterNormal: Greater -> type(Greater);
LessOrEqualNormal: LessOrEqual -> type(LessOrEqual);
GreaterOrEqualNormal: GreaterOrEqual -> type(GreaterOrEqual);
ApproximatelyEqualNormal:
	ApproximatelyEqual -> type(ApproximatelyEqual);
MinusNormal: Minus -> type(Minus);
PlusNormal: Plus -> type(Plus);
MulNormal: Mul -> type(Mul);
DivNormal: Div -> type(Div);
IDivNormal: IDiv -> type(IDiv);
ModNormal: Mod -> type(Mod);
HashNormal: Hash -> type(Hash);
LParenNormal:
	LParen -> type(LParen), pushMode(NormalCodeInside);
RParenNormal: RParen -> type(RParen);
LSquareBracketNormal:
	LSquareBracket -> type(LSquareBracket), pushMode(NormalCodeInside);
RSquareBracketNormal: RSquareBracket -> type(RSquareBracket);
LBraceNormal:
	LBrace -> type(LBrace), pushMode(NormalCodeInside);
RBraceNormal: RBrace -> type(RBrace);
IdentifierNormal: Identifier -> type(Identifier);
AddrOfNormal: AddrOf -> type(AddrOf);
SpaceNormal: Space -> skip;
UnknownNormal: Unknown -> type(Unknown);

mode NormalCodeInside;
StringLiteralNormalInside:
	StringLiteral -> type(StringLiteral);
NewlineNormalInside:
	Newline -> type(Newline), popMode, mode(DEFAULT_MODE);
CommentPrefixNormalInside:
	CommentPrefix -> skip, mode(InsideComment);
NumberLiteralNormalInside:
	NumberLiteral -> type(NumberLiteral);
DateTimeLiteralNormalInside: DateTimeLiteral -> type(DateTimeLiteral);
DotNormalInside: Dot -> type(Dot);
AndNormalInside: And -> type(And);
OrNormalInside: Or -> type(Or);
CommaNormalInside: Comma -> type(Comma);
EqualNormalInside: ('=' | '＝' | '==') -> type(Equal);
NotEqualNormalInside: NotEqual -> type(NotEqual);
LessNormalInside: Less -> type(Less);
GreaterNormalInside: Greater -> type(Greater);
LessOrEqualNormalInside: LessOrEqual -> type(LessOrEqual);
GreaterOrEqualNormalInside:
	GreaterOrEqual -> type(GreaterOrEqual);
ApproximatelyEqualNormalInside:
	ApproximatelyEqual -> type(ApproximatelyEqual);
MinusNormalInside: Minus -> type(Minus);
PlusNormalInside: Plus -> type(Plus);
MulNormalInside: Mul -> type(Mul);
DivNormalInside: Div -> type(Div);
IDivNormalInside: IDiv -> type(IDiv);
ModNormalInside: Mod -> type(Mod);
HashNormalInside: Hash -> type(Hash);
LParenNormalInside:
	LParen -> type(LParen), pushMode(NormalCodeInside);
RParenNormalInside: RParen -> type(RParen), popMode;
LSquareBracketNormalInside:
	LSquareBracket -> type(LSquareBracket), pushMode(NormalCodeInside);
RSquareBracketNormalInside:
	RSquareBracket -> type(RSquareBracket), popMode;
LBraceNormalInside:
	LBrace -> type(LBrace), pushMode(NormalCodeInside);
RBraceNormalInside: RBrace -> type(RBrace), popMode;
IdentifierNormalInside: Identifier -> type(Identifier);
AddrOfNormalInside: AddrOf -> type(AddrOf);
SpaceNormalInside: Space -> skip;
UnknownNormalInside: Unknown -> type(Unknown);

// ClassAttr
mode ClassAttr1;
Class1Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(ClassAttr2);
Class1Identifier: Identifier -> type(Identifier);
Class1Newline: Newline -> type(Newline), mode(DEFAULT_MODE);
mode ClassAttr2;
Class2Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(ClassAttr3);
Object: '<对象>';
WinPrefix: '窗口' SpaceChar* ':' SpaceChar*;
Class2Identifier: Identifier -> type(Identifier);
Class2Newline: Newline -> type(Newline), mode(DEFAULT_MODE);
mode ClassAttr3;
Class3Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(InsideComment);
Class3Space: Space -> skip;
Class3Public: '公开' -> type(Public);
Class3Newline: Newline -> type(Newline), mode(DEFAULT_MODE);

// MethodAttr
mode MethodAttr1;
Method1Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(MethodAttr2);
Method1Identifier: Identifier -> type(Identifier);
Method1Newline: Newline -> type(Newline), mode(DEFAULT_MODE);
mode MethodAttr2;
Method2Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(MethodAttr3);
Method2Identifier: Identifier -> type(Identifier);
Method2Newline: Newline -> type(Newline), mode(DEFAULT_MODE);
mode MethodAttr3;
Method3Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(InsideComment);
Method3Space: Space -> skip;
Method3Public: '公开' -> type(Public);
Method3Newline: Newline -> type(Newline), mode(DEFAULT_MODE);

// ArgAttr
mode ArgAttr1;
Arg1Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(ArgAttr2);
Arg1Identifier: Identifier -> type(Identifier);
Arg1Newline: Newline -> type(Newline), mode(DEFAULT_MODE);
mode ArgAttr2;
Arg2Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(ArgAttr3);
Arg2Identifier: Identifier -> type(Identifier);
Arg2Newline: Newline -> type(Newline), mode(DEFAULT_MODE);
mode ArgAttr3;
Arg3Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(InsideComment);
Arg3Space: Space -> skip;
Arg3Optional: '可空' -> type(Optional);
Arg3Array: '数组' -> type(Array);
Arg3ByRef: ('参考' | '传址') -> type(ByRef);
Arg3Newline: Newline -> type(Newline), mode(DEFAULT_MODE);

// VariableAttr
mode VariableAttr1;
Variable1Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(VariableAttr2);
Variable1Identifier: Identifier -> type(Identifier);
Variable1Newline:
	Newline -> type(Newline), mode(DEFAULT_MODE);
mode VariableAttr2;
Variable2Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(VariableAttr3);
Variable2Identifier: Identifier -> type(Identifier);
Variable2Newline:
	Newline -> type(Newline), mode(DEFAULT_MODE);
mode VariableAttr3;
Variable3Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(VariableAttr4);
Variable3Space: Space -> skip;
Variable3Static: '静态' -> type(Static);
Variable3Public: '公开' -> type(Public);
Variable3ByRef: '传址' -> type(ByRef);
Variable3Newline:
	Newline -> type(Newline), mode(DEFAULT_MODE);
mode VariableAttr4;
Variable4Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(InsideComment);
Variable4StringItem: StringItem -> type(StringItem);
Variable4Newline:
	Newline -> type(Newline), mode(DEFAULT_MODE);

// StructAttr
mode StructAttr1;
Struct1Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(StructAttr2);
Struct1Identifier: Identifier -> type(Identifier);
Struct1Newline: Newline -> type(Newline), mode(DEFAULT_MODE);
mode StructAttr2;
Struct3Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(InsideComment);
Struct3Space: Space -> skip;
Struct3Public: '公开' -> type(Public);
Struct3Newline: Newline -> type(Newline), mode(DEFAULT_MODE);

// DllDeclareAttr
mode DllDeclareAttr1;
DllDeclare1Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(DllDeclareAttr2);
DllDeclare1Identifier: Identifier -> type(Identifier);
DllDeclare1Newline:
	Newline -> type(Newline), mode(DEFAULT_MODE);
mode DllDeclareAttr2;
DllDeclare2Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(DllDeclareAttr3);
DllDeclare2Identifier: Identifier -> type(Identifier);
DllDeclare2Newline:
	Newline -> type(Newline), mode(DEFAULT_MODE);
mode DllDeclareAttr3;
DllDeclare3Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(DllDeclareAttr4);
DllDeclare3StringItem: StringItem -> type(StringItem);
DllDeclare3Newline:
	Newline -> type(Newline), mode(DEFAULT_MODE);
mode DllDeclareAttr4;
DllDeclare4Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(DllDeclareAttr5);
DllDeclare4StringItem: StringItem -> type(StringItem);
DllDeclare4Newline:
	Newline -> type(Newline), mode(DEFAULT_MODE);
mode DllDeclareAttr5;
DllDeclare5Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(InsideComment);
DllDeclare5Space: Space -> skip;
DllDeclare5Public: '公开' -> type(Public);
DllDeclare5Newline:
	Newline -> type(Newline), mode(DEFAULT_MODE);

// ConstantAttr
mode ConstantAttr1;
Constant1Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(ConstantAttr2);
Constant1Identifier: Identifier -> type(Identifier);
Constant1Newline:
	Newline -> type(Newline), mode(DEFAULT_MODE);
mode ConstantAttr2;
Constant2Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(ConstantAttr3);
Constant2StringItem: StringItem -> type(StringItem);
Constant2Newline:
	Newline -> type(Newline), mode(DEFAULT_MODE);
mode ConstantAttr3;
Constant3Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(InsideComment);
Constant3Space: Space -> skip;
Constant3Public: '公开' -> type(Public);
Constant3Newline:
	Newline -> type(Newline), mode(DEFAULT_MODE);

// ResourceAttr
mode ResourceAttr1;
Resource1Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(ResourceAttr2);
Resource1Identifier: Identifier -> type(Identifier);
Resource1Newline:
	Newline -> type(Newline), mode(DEFAULT_MODE);
mode ResourceAttr2;
Resource2Separator:
	SpaceChar* Comma SpaceChar* -> type(Separator), mode(InsideComment);
Resource2Space: Space -> skip;
Resource2Public: '公开' -> type(Public);
Resource2Newline:
	Newline -> type(Newline), mode(DEFAULT_MODE);

mode InsideComment;
Comment: ~ ('\n' | '\r')+;
InsideCommentNewline:
	Newline -> type(Newline), mode(DEFAULT_MODE);
