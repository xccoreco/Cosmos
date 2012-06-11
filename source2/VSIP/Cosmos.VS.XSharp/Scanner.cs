﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using XSC = Cosmos.Compiler.XSharp;

namespace Cosmos.VS.XSharp {
  internal class Scanner : IScanner {
    struct TokenData {
      public TokenType Type;
      public TokenColor Color;
    }

    IVsTextBuffer mBuffer;
    XSC.Parser mParser;
    int mTokenIdx;
    static TokenData[] mTokenMap;

    static Scanner() {
      int xEnumMax = Enum.GetValues(typeof(XSC.TokenType)).Cast<int>().Max();
      mTokenMap = new TokenData[xEnumMax + 1];
      for(int i = 0; i < xEnumMax; i++) {
        mTokenMap[i].Type = TokenType.Unknown;
        mTokenMap[i].Color = TokenColor.Text;
      }

      mTokenMap[(int)XSC.TokenType.Label] = new TokenData { Type = TokenType.Identifier, Color = TokenColor.Identifier };
      mTokenMap[(int)XSC.TokenType.Comment] = new TokenData { Type = TokenType.LineComment, Color = TokenColor.Comment };
      mTokenMap[(int)XSC.TokenType.Literal] = new TokenData { Type = TokenType.Literal , Color = TokenColor.String };
      mTokenMap[(int)XSC.TokenType.Register] = new TokenData { Type = TokenType.Keyword , Color = TokenColor.Keyword };
      mTokenMap[(int)XSC.TokenType.Op] = new TokenData { Type = TokenType.Keyword , Color = TokenColor.Keyword };
      mTokenMap[(int)XSC.TokenType.Assignment] = new TokenData { Type = TokenType.Operator , Color = TokenColor.Text };
      mTokenMap[(int)XSC.TokenType.ValueNumber] = new TokenData { Type = TokenType.Literal , Color = TokenColor.Number };
      mTokenMap[(int)XSC.TokenType.BracketLeft] = new TokenData { Type = TokenType.Delimiter , Color = TokenColor.Text };
      mTokenMap[(int)XSC.TokenType.BracketRight] = new TokenData { Type = TokenType.Delimiter , Color = TokenColor.Text };
      mTokenMap[(int)XSC.TokenType.Plus] = new TokenData { Type = TokenType.Operator , Color = TokenColor.Text };
      mTokenMap[(int)XSC.TokenType.Minus] = new TokenData { Type = TokenType.Operator , Color = TokenColor.Text };
      mTokenMap[(int)XSC.TokenType.Inc] = new TokenData { Type = TokenType.Operator , Color = TokenColor.Text };
      mTokenMap[(int)XSC.TokenType.Dec] = new TokenData { Type = TokenType.Operator , Color = TokenColor.Text };
    }

    public Scanner(IVsTextBuffer aBuffer) {
      mBuffer = aBuffer;
    }

    // State argument: http://social.msdn.microsoft.com/Forums/en-US/vsx/thread/38939d76-6f8b-473f-9ee1-fc3ae7b59cce
    bool IScanner.ScanTokenAndProvideInfoAboutIt(TokenInfo aTokenInfo, ref int aState) {
      if (mTokenIdx == mParser.Tokens.Count) {
        return false;
      }

      var xToken = mParser.Tokens[mTokenIdx];
      mTokenIdx++;

      aTokenInfo.Token = (int)xToken.Type;
      aTokenInfo.StartIndex = xToken.SrcPosStart;
      aTokenInfo.EndIndex = xToken.SrcPosEnd;

      var xTokenData = mTokenMap[(int)xToken.Type];
      aTokenInfo.Type = xTokenData.Type;
      aTokenInfo.Color = xTokenData.Color;

      return true;
    }

    void IScanner.SetSource(string aSource, int aOffset) {
      mTokenIdx = 0;
      mParser = new XSC.Parser(aSource.Substring(aOffset));
    }
  }
}