parser grammar EplParser;
options {
	tokenVocab = EplLexer;
}

start: Newline* elem (Newline+ elem)* Newline* EOF;
elem:
	globalVariableElem
	| structElem
	| classElem
	| dllDeclareElem
	| constantElem
	| imageResourceElem
	| soundResourceElem
	| textResourceElem;
globalVariableElem:
	Global name=Identifier? (
		Separator dataType=Identifier? (
			Separator Public? (
				Separator array=StringItem? (Separator Comment?)?
			)?
		)?
	)?;
localVariableElem:
	Local name=Identifier? (
		Separator dataType=Identifier? (
			Separator Static? (
				Separator array=StringItem? (Separator Comment?)?
			)?
		)?
	)?;
classVariableElem:
	ClassVariable name=Identifier? (
		Separator dataType=Identifier? (
			Separator (
				Separator array=StringItem? (Separator Comment?)?
			)?
		)?
	)?;
classBase: (Object | WinPrefix? Identifier);
classElem:
	Class name=Identifier? (
		Separator base=classBase? (
			Separator Public? (Separator Comment?)?
		)?
	)? (Newline+ classVariableElem)* (Newline+ methodElem)*;
methodElem:
	Method name=Identifier? (
		Separator returnDataType=Identifier? (
			Separator Public? (Separator Comment?)?
		)?
	)? (Newline+ argElem)* (Newline+ localVariableElem)*
	// assuming all locals are defined on the top of the method
	Newline?
	// it's a tradition to add a empty line before the actual code, and it should not be recognized as a empty statement.
	statementBlock;
argElem:
	Arg name=Identifier? (
		Separator dataType=Identifier? (
			Separator (ByRef | Optional | Array)* (
				Separator Comment?
			)?
		)?
	)?;
dllDeclareElem:
	DllDeclare name=Identifier? (
		Separator returnDataType=Identifier? (
			Separator libraryName=StringItem? (
				Separator entryPoint=StringItem? (
					Separator Public? (Separator Comment?)?
				)?
			)?
		)?
	)? (Newline+ argElem)*;
constantElem:
	Constant name=Identifier? (
		Separator value=StringItem? (
			Separator Public? (Separator Comment?)?
		)?
	)?;
structElem:
	Struct name=Identifier? (Separator Public? (Separator Comment?)?)? (
		Newline+ structMemberElem
	)*;
structMemberElem:
	StructMember name=Identifier? (
		Separator dataType=Identifier? (
			Separator ByRef? (
				Separator array=StringItem? (Separator Comment?)?
			)?
		)?
	)?;
imageResourceElem:
	ImageResource name=Identifier? (
		Separator Public? (Separator Comment?)?
	)?;
soundResourceElem:
	SoundResource name=Identifier? (
		Separator Public? (Separator Comment?)?
	)?;
textResourceElem:
	TextResource name=Identifier? (
		Separator Public? (Separator Comment?)?
	)?;
expression:
	LParen expression RParen										        # ParenthesizedExpression
	| StringLiteral													        # StringLiteralExpression
	| NumberLiteral													        # NumberLiteralExpression
	| DateTimeLiteral                                                       # DateTimeLiteralExpression
	| AddrOf Identifier                                                     # AddrOfExpression
	| Hash dataType = Identifier Dot name = Identifier							# EnumExpression
	| Hash name = Identifier												# ConstantExpression
	| Identifier													        # VariableExpression
	| target = expression Dot method = Identifier arguments			        # MemberCallExpression
	| target = expression Dot member = Identifier					        # AccessMemberExpression
	| method = Identifier arguments									        # NormalCallExpression
	| target = expression LSquareBracket index = expression RSquareBracket	# AccessArrayExpression
	| LBrace expressionList? RBrace									        # ArrayLiteralExpression
	| operator = Minus operand = expression							        # UnaryOperatorExpression
	| lhs = expression operator = (Mul | Div) rhs = expression		        # BinaryOperatorExpression
	| lhs = expression operator = IDiv rhs = expression				        # BinaryOperatorExpression
	| lhs = expression operator = Mod rhs = expression				        # BinaryOperatorExpression
	| lhs = expression operator = (Plus | Minus) rhs = expression	        # BinaryOperatorExpression
	| lhs = expression operator = (
		Equal
		| NotEqual
		| Greater
		| Less
		| GreaterOrEqual
		| LessOrEqual
		| ApproximatelyEqual
	) rhs = expression														# BinaryOperatorExpression
	| lhs = expression operator = And rhs = expression						# BinaryOperatorExpression
	| lhs = expression operator = Or rhs = expression						# BinaryOperatorExpression
	| <assoc = right> lhs = expression operator = Assign rhs = expression	# BinaryOperatorExpression;
expressionList: expression (Comma expression)*;
nothing: ;
expressionOrNothing: expression | nothing;
arguments: LParen expressionOrNothing (Comma expressionOrNothing)* RParen;
expressionStatement: expression Comment?;
ifTrueStatement:
	IfTrue arguments comment = Comment? onTrue = statementBlock Newline EndIfTrue;
ifStatement:
	If arguments comment = Comment? onTrue = statementBlock Newline Else onFalse = statementBlock Newline
		EndIf;
whenIf: arguments comment = Comment? block = statementBlock;
whenElse: block = statementBlock;
whenStatement:
	BeginWhen whenIf (Newline WhenCase whenIf)* (
		Newline WhenElse whenElse
	)? Newline EndWhen;
counterStatement:
	Counter argumentsOnStart = arguments commentOnStart = Comment? block = statementBlock Newline EndCounter
		argumentsOnEnd = arguments commentOnEnd = Comment?;
forStatement:
	For argumentsOnStart = arguments commentOnStart = Comment? block = statementBlock Newline EndFor
		argumentsOnEnd = arguments commentOnEnd = Comment?;
doWhileStatement:
	DoWhile argumentsOnStart = arguments commentOnStart = Comment? block = statementBlock Newline EndDoWhile
		argumentsOnEnd = arguments commentOnEnd = Comment?;
whileStatement:
	While argumentsOnStart = arguments commentOnStart = Comment? block = statementBlock Newline EndWhile
		argumentsOnEnd = arguments commentOnEnd = Comment?;
emptyStatement: Comment?;
invaildStatement: ~ (
	Newline
	| Class
	| Method
	| Arg
	| Local
	| ClassVariable
	| Global
	| Struct
	| StructMember
	| DllDeclare
	| Constant
	| ImageResource
	| SoundResource
	| TextResource
	| IfTrue
	| EndIfTrue
	| If
	| Else
	| EndIf
	| For
	| EndFor
	| Counter
	| EndCounter
	| While
	| EndWhile
	| DoWhile
	| EndDoWhile
	| BeginWhen
	| WhenCase
	| WhenElse
	| EndWhen
) (~Newline)*;
statement:
	ifTrueStatement
	| ifStatement
	| whenStatement
	| counterStatement
	| forStatement
	| doWhileStatement
	| whileStatement
	| expressionStatement
	| emptyStatement
	| invaildStatement;
statementBlock: (Newline statement)*;