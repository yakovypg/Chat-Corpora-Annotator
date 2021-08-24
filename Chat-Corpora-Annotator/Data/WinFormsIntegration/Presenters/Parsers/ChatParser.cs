//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.7.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from Chat.g4 by ANTLR 4.7.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.IO;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.7.1")]
[System.CLSCompliant(false)]
public partial class ChatParser : Parser {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		T__0=1, T__1=2, T__2=3, T__3=4, Select=5, InWin=6, Not=7, And=8, Or=9, 
		HasWordOfDict=10, HasTime=11, HasLocation=12, HasOrganization=13, HasURL=14, 
		HasDate=15, HasQuestion=16, HasUserMentioned=17, ByUser=18, INTEGER=19, 
		STRING=20, WS=21;
	public const int
		RULE_query = 0, RULE_body = 1, RULE_query_seq = 2, RULE_restrictions = 3, 
		RULE_restriction = 4, RULE_condition = 5, RULE_number = 6, RULE_hdict = 7, 
		RULE_huser = 8;
	public static readonly string[] ruleNames = {
		"query", "body", "query_seq", "restrictions", "restriction", "condition", 
		"number", "hdict", "huser"
	};

	private static readonly string[] _LiteralNames = {
		null, "';'", "'('", "')'", "','"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, null, null, "Select", "InWin", "Not", "And", "Or", "HasWordOfDict", 
		"HasTime", "HasLocation", "HasOrganization", "HasURL", "HasDate", "HasQuestion", 
		"HasUserMentioned", "ByUser", "INTEGER", "STRING", "WS"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "Chat.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string SerializedAtn { get { return new string(_serializedATN); } }

	static ChatParser() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}

		public ChatParser(ITokenStream input) : this(input, Console.Out, Console.Error) { }

		public ChatParser(ITokenStream input, TextWriter output, TextWriter errorOutput)
		: base(input, output, errorOutput)
	{
		Interpreter = new ParserATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}
	public partial class QueryContext : ParserRuleContext {
		public ITerminalNode Select() { return GetToken(ChatParser.Select, 0); }
		public BodyContext body() {
			return GetRuleContext<BodyContext>(0);
		}
		public QueryContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_query; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IChatVisitor<TResult> typedVisitor = visitor as IChatVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitQuery(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public QueryContext query() {
		QueryContext _localctx = new QueryContext(Context, State);
		EnterRule(_localctx, 0, RULE_query);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 18; Match(Select);
			State = 19; body();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class BodyContext : ParserRuleContext {
		public Query_seqContext query_seq() {
			return GetRuleContext<Query_seqContext>(0);
		}
		public RestrictionsContext restrictions() {
			return GetRuleContext<RestrictionsContext>(0);
		}
		public ITerminalNode InWin() { return GetToken(ChatParser.InWin, 0); }
		public NumberContext number() {
			return GetRuleContext<NumberContext>(0);
		}
		public BodyContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_body; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IChatVisitor<TResult> typedVisitor = visitor as IChatVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitBody(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public BodyContext body() {
		BodyContext _localctx = new BodyContext(Context, State);
		EnterRule(_localctx, 2, RULE_body);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 23;
			ErrorHandler.Sync(this);
			switch ( Interpreter.AdaptivePredict(TokenStream,0,Context) ) {
			case 1:
				{
				State = 21; query_seq();
				}
				break;
			case 2:
				{
				State = 22; restrictions();
				}
				break;
			}
			State = 26;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			if (_la==T__0) {
				{
				State = 25; Match(T__0);
				}
			}

			State = 30;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			if (_la==InWin) {
				{
				State = 28; Match(InWin);
				State = 29; number();
				}
			}

			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class Query_seqContext : ParserRuleContext {
		public QueryContext[] query() {
			return GetRuleContexts<QueryContext>();
		}
		public QueryContext query(int i) {
			return GetRuleContext<QueryContext>(i);
		}
		public Query_seqContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_query_seq; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IChatVisitor<TResult> typedVisitor = visitor as IChatVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitQuery_seq(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public Query_seqContext query_seq() {
		Query_seqContext _localctx = new Query_seqContext(Context, State);
		EnterRule(_localctx, 4, RULE_query_seq);
		try {
			int _alt;
			EnterOuterAlt(_localctx, 1);
			{
			State = 32; Match(T__1);
			State = 33; query();
			State = 34; Match(T__2);
			State = 42;
			ErrorHandler.Sync(this);
			_alt = Interpreter.AdaptivePredict(TokenStream,3,Context);
			while ( _alt!=2 && _alt!=global::Antlr4.Runtime.Atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					State = 35; Match(T__0);
					State = 36; Match(T__1);
					State = 37; query();
					State = 38; Match(T__2);
					}
					} 
				}
				State = 44;
				ErrorHandler.Sync(this);
				_alt = Interpreter.AdaptivePredict(TokenStream,3,Context);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class RestrictionsContext : ParserRuleContext {
		public RestrictionContext[] restriction() {
			return GetRuleContexts<RestrictionContext>();
		}
		public RestrictionContext restriction(int i) {
			return GetRuleContext<RestrictionContext>(i);
		}
		public RestrictionsContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_restrictions; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IChatVisitor<TResult> typedVisitor = visitor as IChatVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitRestrictions(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public RestrictionsContext restrictions() {
		RestrictionsContext _localctx = new RestrictionsContext(Context, State);
		EnterRule(_localctx, 6, RULE_restrictions);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 45; restriction(0);
			State = 50;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==T__3) {
				{
				{
				State = 46; Match(T__3);
				State = 47; restriction(0);
				}
				}
				State = 52;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class RestrictionContext : ParserRuleContext {
		public RestrictionContext[] restriction() {
			return GetRuleContexts<RestrictionContext>();
		}
		public RestrictionContext restriction(int i) {
			return GetRuleContext<RestrictionContext>(i);
		}
		public ITerminalNode Not() { return GetToken(ChatParser.Not, 0); }
		public ConditionContext condition() {
			return GetRuleContext<ConditionContext>(0);
		}
		public ITerminalNode And() { return GetToken(ChatParser.And, 0); }
		public ITerminalNode Or() { return GetToken(ChatParser.Or, 0); }
		public RestrictionContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_restriction; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IChatVisitor<TResult> typedVisitor = visitor as IChatVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitRestriction(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public RestrictionContext restriction() {
		return restriction(0);
	}

	private RestrictionContext restriction(int _p) {
		ParserRuleContext _parentctx = Context;
		int _parentState = State;
		RestrictionContext _localctx = new RestrictionContext(Context, _parentState);
		RestrictionContext _prevctx = _localctx;
		int _startState = 8;
		EnterRecursionRule(_localctx, 8, RULE_restriction, _p);
		try {
			int _alt;
			EnterOuterAlt(_localctx, 1);
			{
			State = 61;
			ErrorHandler.Sync(this);
			switch (TokenStream.LA(1)) {
			case T__1:
				{
				State = 54; Match(T__1);
				State = 55; restriction(0);
				State = 56; Match(T__2);
				}
				break;
			case Not:
				{
				State = 58; Match(Not);
				State = 59; restriction(2);
				}
				break;
			case HasWordOfDict:
			case HasTime:
			case HasLocation:
			case HasOrganization:
			case HasURL:
			case HasDate:
			case HasQuestion:
			case HasUserMentioned:
			case ByUser:
				{
				State = 60; condition();
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			Context.Stop = TokenStream.LT(-1);
			State = 71;
			ErrorHandler.Sync(this);
			_alt = Interpreter.AdaptivePredict(TokenStream,7,Context);
			while ( _alt!=2 && _alt!=global::Antlr4.Runtime.Atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					if ( ParseListeners!=null )
						TriggerExitRuleEvent();
					_prevctx = _localctx;
					{
					State = 69;
					ErrorHandler.Sync(this);
					switch ( Interpreter.AdaptivePredict(TokenStream,6,Context) ) {
					case 1:
						{
						_localctx = new RestrictionContext(_parentctx, _parentState);
						PushNewRecursionContext(_localctx, _startState, RULE_restriction);
						State = 63;
						if (!(Precpred(Context, 5))) throw new FailedPredicateException(this, "Precpred(Context, 5)");
						State = 64; Match(And);
						State = 65; restriction(6);
						}
						break;
					case 2:
						{
						_localctx = new RestrictionContext(_parentctx, _parentState);
						PushNewRecursionContext(_localctx, _startState, RULE_restriction);
						State = 66;
						if (!(Precpred(Context, 4))) throw new FailedPredicateException(this, "Precpred(Context, 4)");
						State = 67; Match(Or);
						State = 68; restriction(5);
						}
						break;
					}
					} 
				}
				State = 73;
				ErrorHandler.Sync(this);
				_alt = Interpreter.AdaptivePredict(TokenStream,7,Context);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			UnrollRecursionContexts(_parentctx);
		}
		return _localctx;
	}

	public partial class ConditionContext : ParserRuleContext {
		public ITerminalNode HasWordOfDict() { return GetToken(ChatParser.HasWordOfDict, 0); }
		public HdictContext hdict() {
			return GetRuleContext<HdictContext>(0);
		}
		public ITerminalNode HasTime() { return GetToken(ChatParser.HasTime, 0); }
		public ITerminalNode HasLocation() { return GetToken(ChatParser.HasLocation, 0); }
		public ITerminalNode HasOrganization() { return GetToken(ChatParser.HasOrganization, 0); }
		public ITerminalNode HasURL() { return GetToken(ChatParser.HasURL, 0); }
		public ITerminalNode HasDate() { return GetToken(ChatParser.HasDate, 0); }
		public ITerminalNode HasQuestion() { return GetToken(ChatParser.HasQuestion, 0); }
		public ITerminalNode HasUserMentioned() { return GetToken(ChatParser.HasUserMentioned, 0); }
		public HuserContext huser() {
			return GetRuleContext<HuserContext>(0);
		}
		public ITerminalNode ByUser() { return GetToken(ChatParser.ByUser, 0); }
		public ConditionContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_condition; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IChatVisitor<TResult> typedVisitor = visitor as IChatVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitCondition(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public ConditionContext condition() {
		ConditionContext _localctx = new ConditionContext(Context, State);
		EnterRule(_localctx, 10, RULE_condition);
		try {
			State = 107;
			ErrorHandler.Sync(this);
			switch (TokenStream.LA(1)) {
			case HasWordOfDict:
				EnterOuterAlt(_localctx, 1);
				{
				State = 74; Match(HasWordOfDict);
				State = 75; Match(T__1);
				State = 76; hdict();
				State = 77; Match(T__2);
				}
				break;
			case HasTime:
				EnterOuterAlt(_localctx, 2);
				{
				State = 79; Match(HasTime);
				State = 80; Match(T__1);
				State = 81; Match(T__2);
				}
				break;
			case HasLocation:
				EnterOuterAlt(_localctx, 3);
				{
				State = 82; Match(HasLocation);
				State = 83; Match(T__1);
				State = 84; Match(T__2);
				}
				break;
			case HasOrganization:
				EnterOuterAlt(_localctx, 4);
				{
				State = 85; Match(HasOrganization);
				State = 86; Match(T__1);
				State = 87; Match(T__2);
				}
				break;
			case HasURL:
				EnterOuterAlt(_localctx, 5);
				{
				State = 88; Match(HasURL);
				State = 89; Match(T__1);
				State = 90; Match(T__2);
				}
				break;
			case HasDate:
				EnterOuterAlt(_localctx, 6);
				{
				State = 91; Match(HasDate);
				State = 92; Match(T__1);
				State = 93; Match(T__2);
				}
				break;
			case HasQuestion:
				EnterOuterAlt(_localctx, 7);
				{
				State = 94; Match(HasQuestion);
				State = 95; Match(T__1);
				State = 96; Match(T__2);
				}
				break;
			case HasUserMentioned:
				EnterOuterAlt(_localctx, 8);
				{
				State = 97; Match(HasUserMentioned);
				State = 98; Match(T__1);
				State = 99; huser();
				State = 100; Match(T__2);
				}
				break;
			case ByUser:
				EnterOuterAlt(_localctx, 9);
				{
				State = 102; Match(ByUser);
				State = 103; Match(T__1);
				State = 104; huser();
				State = 105; Match(T__2);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class NumberContext : ParserRuleContext {
		public ITerminalNode INTEGER() { return GetToken(ChatParser.INTEGER, 0); }
		public NumberContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_number; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IChatVisitor<TResult> typedVisitor = visitor as IChatVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitNumber(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public NumberContext number() {
		NumberContext _localctx = new NumberContext(Context, State);
		EnterRule(_localctx, 12, RULE_number);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 109; Match(INTEGER);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class HdictContext : ParserRuleContext {
		public ITerminalNode STRING() { return GetToken(ChatParser.STRING, 0); }
		public HdictContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_hdict; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IChatVisitor<TResult> typedVisitor = visitor as IChatVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitHdict(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public HdictContext hdict() {
		HdictContext _localctx = new HdictContext(Context, State);
		EnterRule(_localctx, 14, RULE_hdict);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 111; Match(STRING);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class HuserContext : ParserRuleContext {
		public ITerminalNode STRING() { return GetToken(ChatParser.STRING, 0); }
		public HuserContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_huser; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IChatVisitor<TResult> typedVisitor = visitor as IChatVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitHuser(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public HuserContext huser() {
		HuserContext _localctx = new HuserContext(Context, State);
		EnterRule(_localctx, 16, RULE_huser);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 113; Match(STRING);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public override bool Sempred(RuleContext _localctx, int ruleIndex, int predIndex) {
		switch (ruleIndex) {
		case 4: return restriction_sempred((RestrictionContext)_localctx, predIndex);
		}
		return true;
	}
	private bool restriction_sempred(RestrictionContext _localctx, int predIndex) {
		switch (predIndex) {
		case 0: return Precpred(Context, 5);
		case 1: return Precpred(Context, 4);
		}
		return true;
	}

	private static char[] _serializedATN = {
		'\x3', '\x608B', '\xA72A', '\x8133', '\xB9ED', '\x417C', '\x3BE7', '\x7786', 
		'\x5964', '\x3', '\x17', 'v', '\x4', '\x2', '\t', '\x2', '\x4', '\x3', 
		'\t', '\x3', '\x4', '\x4', '\t', '\x4', '\x4', '\x5', '\t', '\x5', '\x4', 
		'\x6', '\t', '\x6', '\x4', '\a', '\t', '\a', '\x4', '\b', '\t', '\b', 
		'\x4', '\t', '\t', '\t', '\x4', '\n', '\t', '\n', '\x3', '\x2', '\x3', 
		'\x2', '\x3', '\x2', '\x3', '\x3', '\x3', '\x3', '\x5', '\x3', '\x1A', 
		'\n', '\x3', '\x3', '\x3', '\x5', '\x3', '\x1D', '\n', '\x3', '\x3', '\x3', 
		'\x3', '\x3', '\x5', '\x3', '!', '\n', '\x3', '\x3', '\x4', '\x3', '\x4', 
		'\x3', '\x4', '\x3', '\x4', '\x3', '\x4', '\x3', '\x4', '\x3', '\x4', 
		'\x3', '\x4', '\a', '\x4', '+', '\n', '\x4', '\f', '\x4', '\xE', '\x4', 
		'.', '\v', '\x4', '\x3', '\x5', '\x3', '\x5', '\x3', '\x5', '\a', '\x5', 
		'\x33', '\n', '\x5', '\f', '\x5', '\xE', '\x5', '\x36', '\v', '\x5', '\x3', 
		'\x6', '\x3', '\x6', '\x3', '\x6', '\x3', '\x6', '\x3', '\x6', '\x3', 
		'\x6', '\x3', '\x6', '\x3', '\x6', '\x5', '\x6', '@', '\n', '\x6', '\x3', 
		'\x6', '\x3', '\x6', '\x3', '\x6', '\x3', '\x6', '\x3', '\x6', '\x3', 
		'\x6', '\a', '\x6', 'H', '\n', '\x6', '\f', '\x6', '\xE', '\x6', 'K', 
		'\v', '\x6', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', 
		'\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', 
		'\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', 
		'\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', 
		'\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', 
		'\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', 
		'\x3', '\a', '\x5', '\a', 'n', '\n', '\a', '\x3', '\b', '\x3', '\b', '\x3', 
		'\t', '\x3', '\t', '\x3', '\n', '\x3', '\n', '\x3', '\n', '\x2', '\x3', 
		'\n', '\v', '\x2', '\x4', '\x6', '\b', '\n', '\f', '\xE', '\x10', '\x12', 
		'\x2', '\x2', '\x2', '}', '\x2', '\x14', '\x3', '\x2', '\x2', '\x2', '\x4', 
		'\x19', '\x3', '\x2', '\x2', '\x2', '\x6', '\"', '\x3', '\x2', '\x2', 
		'\x2', '\b', '/', '\x3', '\x2', '\x2', '\x2', '\n', '?', '\x3', '\x2', 
		'\x2', '\x2', '\f', 'm', '\x3', '\x2', '\x2', '\x2', '\xE', 'o', '\x3', 
		'\x2', '\x2', '\x2', '\x10', 'q', '\x3', '\x2', '\x2', '\x2', '\x12', 
		's', '\x3', '\x2', '\x2', '\x2', '\x14', '\x15', '\a', '\a', '\x2', '\x2', 
		'\x15', '\x16', '\x5', '\x4', '\x3', '\x2', '\x16', '\x3', '\x3', '\x2', 
		'\x2', '\x2', '\x17', '\x1A', '\x5', '\x6', '\x4', '\x2', '\x18', '\x1A', 
		'\x5', '\b', '\x5', '\x2', '\x19', '\x17', '\x3', '\x2', '\x2', '\x2', 
		'\x19', '\x18', '\x3', '\x2', '\x2', '\x2', '\x1A', '\x1C', '\x3', '\x2', 
		'\x2', '\x2', '\x1B', '\x1D', '\a', '\x3', '\x2', '\x2', '\x1C', '\x1B', 
		'\x3', '\x2', '\x2', '\x2', '\x1C', '\x1D', '\x3', '\x2', '\x2', '\x2', 
		'\x1D', ' ', '\x3', '\x2', '\x2', '\x2', '\x1E', '\x1F', '\a', '\b', '\x2', 
		'\x2', '\x1F', '!', '\x5', '\xE', '\b', '\x2', ' ', '\x1E', '\x3', '\x2', 
		'\x2', '\x2', ' ', '!', '\x3', '\x2', '\x2', '\x2', '!', '\x5', '\x3', 
		'\x2', '\x2', '\x2', '\"', '#', '\a', '\x4', '\x2', '\x2', '#', '$', '\x5', 
		'\x2', '\x2', '\x2', '$', ',', '\a', '\x5', '\x2', '\x2', '%', '&', '\a', 
		'\x3', '\x2', '\x2', '&', '\'', '\a', '\x4', '\x2', '\x2', '\'', '(', 
		'\x5', '\x2', '\x2', '\x2', '(', ')', '\a', '\x5', '\x2', '\x2', ')', 
		'+', '\x3', '\x2', '\x2', '\x2', '*', '%', '\x3', '\x2', '\x2', '\x2', 
		'+', '.', '\x3', '\x2', '\x2', '\x2', ',', '*', '\x3', '\x2', '\x2', '\x2', 
		',', '-', '\x3', '\x2', '\x2', '\x2', '-', '\a', '\x3', '\x2', '\x2', 
		'\x2', '.', ',', '\x3', '\x2', '\x2', '\x2', '/', '\x34', '\x5', '\n', 
		'\x6', '\x2', '\x30', '\x31', '\a', '\x6', '\x2', '\x2', '\x31', '\x33', 
		'\x5', '\n', '\x6', '\x2', '\x32', '\x30', '\x3', '\x2', '\x2', '\x2', 
		'\x33', '\x36', '\x3', '\x2', '\x2', '\x2', '\x34', '\x32', '\x3', '\x2', 
		'\x2', '\x2', '\x34', '\x35', '\x3', '\x2', '\x2', '\x2', '\x35', '\t', 
		'\x3', '\x2', '\x2', '\x2', '\x36', '\x34', '\x3', '\x2', '\x2', '\x2', 
		'\x37', '\x38', '\b', '\x6', '\x1', '\x2', '\x38', '\x39', '\a', '\x4', 
		'\x2', '\x2', '\x39', ':', '\x5', '\n', '\x6', '\x2', ':', ';', '\a', 
		'\x5', '\x2', '\x2', ';', '@', '\x3', '\x2', '\x2', '\x2', '<', '=', '\a', 
		'\t', '\x2', '\x2', '=', '@', '\x5', '\n', '\x6', '\x4', '>', '@', '\x5', 
		'\f', '\a', '\x2', '?', '\x37', '\x3', '\x2', '\x2', '\x2', '?', '<', 
		'\x3', '\x2', '\x2', '\x2', '?', '>', '\x3', '\x2', '\x2', '\x2', '@', 
		'I', '\x3', '\x2', '\x2', '\x2', '\x41', '\x42', '\f', '\a', '\x2', '\x2', 
		'\x42', '\x43', '\a', '\n', '\x2', '\x2', '\x43', 'H', '\x5', '\n', '\x6', 
		'\b', '\x44', '\x45', '\f', '\x6', '\x2', '\x2', '\x45', '\x46', '\a', 
		'\v', '\x2', '\x2', '\x46', 'H', '\x5', '\n', '\x6', '\a', 'G', '\x41', 
		'\x3', '\x2', '\x2', '\x2', 'G', '\x44', '\x3', '\x2', '\x2', '\x2', 'H', 
		'K', '\x3', '\x2', '\x2', '\x2', 'I', 'G', '\x3', '\x2', '\x2', '\x2', 
		'I', 'J', '\x3', '\x2', '\x2', '\x2', 'J', '\v', '\x3', '\x2', '\x2', 
		'\x2', 'K', 'I', '\x3', '\x2', '\x2', '\x2', 'L', 'M', '\a', '\f', '\x2', 
		'\x2', 'M', 'N', '\a', '\x4', '\x2', '\x2', 'N', 'O', '\x5', '\x10', '\t', 
		'\x2', 'O', 'P', '\a', '\x5', '\x2', '\x2', 'P', 'n', '\x3', '\x2', '\x2', 
		'\x2', 'Q', 'R', '\a', '\r', '\x2', '\x2', 'R', 'S', '\a', '\x4', '\x2', 
		'\x2', 'S', 'n', '\a', '\x5', '\x2', '\x2', 'T', 'U', '\a', '\xE', '\x2', 
		'\x2', 'U', 'V', '\a', '\x4', '\x2', '\x2', 'V', 'n', '\a', '\x5', '\x2', 
		'\x2', 'W', 'X', '\a', '\xF', '\x2', '\x2', 'X', 'Y', '\a', '\x4', '\x2', 
		'\x2', 'Y', 'n', '\a', '\x5', '\x2', '\x2', 'Z', '[', '\a', '\x10', '\x2', 
		'\x2', '[', '\\', '\a', '\x4', '\x2', '\x2', '\\', 'n', '\a', '\x5', '\x2', 
		'\x2', ']', '^', '\a', '\x11', '\x2', '\x2', '^', '_', '\a', '\x4', '\x2', 
		'\x2', '_', 'n', '\a', '\x5', '\x2', '\x2', '`', '\x61', '\a', '\x12', 
		'\x2', '\x2', '\x61', '\x62', '\a', '\x4', '\x2', '\x2', '\x62', 'n', 
		'\a', '\x5', '\x2', '\x2', '\x63', '\x64', '\a', '\x13', '\x2', '\x2', 
		'\x64', '\x65', '\a', '\x4', '\x2', '\x2', '\x65', '\x66', '\x5', '\x12', 
		'\n', '\x2', '\x66', 'g', '\a', '\x5', '\x2', '\x2', 'g', 'n', '\x3', 
		'\x2', '\x2', '\x2', 'h', 'i', '\a', '\x14', '\x2', '\x2', 'i', 'j', '\a', 
		'\x4', '\x2', '\x2', 'j', 'k', '\x5', '\x12', '\n', '\x2', 'k', 'l', '\a', 
		'\x5', '\x2', '\x2', 'l', 'n', '\x3', '\x2', '\x2', '\x2', 'm', 'L', '\x3', 
		'\x2', '\x2', '\x2', 'm', 'Q', '\x3', '\x2', '\x2', '\x2', 'm', 'T', '\x3', 
		'\x2', '\x2', '\x2', 'm', 'W', '\x3', '\x2', '\x2', '\x2', 'm', 'Z', '\x3', 
		'\x2', '\x2', '\x2', 'm', ']', '\x3', '\x2', '\x2', '\x2', 'm', '`', '\x3', 
		'\x2', '\x2', '\x2', 'm', '\x63', '\x3', '\x2', '\x2', '\x2', 'm', 'h', 
		'\x3', '\x2', '\x2', '\x2', 'n', '\r', '\x3', '\x2', '\x2', '\x2', 'o', 
		'p', '\a', '\x15', '\x2', '\x2', 'p', '\xF', '\x3', '\x2', '\x2', '\x2', 
		'q', 'r', '\a', '\x16', '\x2', '\x2', 'r', '\x11', '\x3', '\x2', '\x2', 
		'\x2', 's', 't', '\a', '\x16', '\x2', '\x2', 't', '\x13', '\x3', '\x2', 
		'\x2', '\x2', '\v', '\x19', '\x1C', ' ', ',', '\x34', '?', 'G', 'I', 'm',
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
