using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;

namespace OneLineCalculator
{
    class RegexFormulas
    {

        private static void PrintMatches(MatchCollection matches,string refText = "")
        {
            if (refText.Length != 0)
                Console.WriteLine("ref:{"+refText+"}");
            //foreach (Match m in matches)
            for (int m=0;m<matches.Count;m++)
            {
                Console.WriteLine(String.Format("{0} [{1}]  idx:{2} len:{3}", m, matches[m].Value, matches[m].Index, matches[m].Length));
            }
        }

        public static string ReplaceWithIndex(string text, MatchCollection matches,
            bool replaceByMatchIndex, int matchIndexOffset,
            string startNew = "", string endNew = "",
            string startOld = "", string endOld = "")
        {
            return ReplaceWithIndex(text, matches.Cast<Match>().ToList(), replaceByMatchIndex, matchIndexOffset,
                startNew, endNew, startOld, endOld);
        }

        public static string ReplaceWithIndex(string text, List<Match> matches,
            bool replaceByMatchIndex, int matchIndexOffset,
            string startNew = "", string endNew = "",
            string startOld="", string endOld="")
        {
            string resultText = "";
            int pos = 0;
            for (int i=0;i<matches.Count;i++)
            {
                Match m = matches[i];
                if (pos < m.Index)
                {
                    resultText += text.Substring(pos, m.Index - pos);
                    pos = m.Index;
                }
                else if (pos != 0 && pos == m.Index)
                    resultText += "+";
                string value;
                
                if (replaceByMatchIndex)
                {
                    value = startNew + (i+ matchIndexOffset).ToString() + endNew;
                }
                else
                {
                    value = m.Value;
                    if (startOld.Length != 0 && value.StartsWith(startOld))
                        value = startNew + value.Substring(startOld.Length);
                    if (endOld.Length != 0 && value.EndsWith(endOld))
                        value = value.Substring(0, value.Length - endOld.Length) + endNew;
                }

                resultText += value;
                pos += m.Length;
            }
            if (pos < text.Length)
                resultText += text.Substring(pos);

            return resultText;
        }

        public static double StrToValue(string text)
        {
            try
            {
                if (text.ToLower().Equals("pi"))
                    return Math.PI;
                else if (text.ToLower().Equals("conste") || text.ToLower().Equals("e"))
                    return Math.E;
                else if (text.StartsWith("0x"))
                    return Convert.ToInt64(text.Substring(2), 16);
                else if (text.StartsWith("0b"))
                    return Convert.ToInt64(text.Substring(2), 2);
                else if (text.EndsWith("b"))
                    return Convert.ToInt64(text.Substring(0, text.Length - 1), 2);
                else
                    return Convert.ToDouble(text);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return Double.NaN;
        }

        public static string FixAddAndSub(string op)
        {
            //op = op.Replace("--", "+");
            //op = op.Replace("+-", "-");
            while (op.Length > 1)
            {
                if (op.EndsWith("--"))
                {
                    if (op.Length > 2)
                        op = op.Substring(0, op.Length - 2);
                    else
                        op = "+";
                }
                else if (op.EndsWith("+-"))
                {
                    op = op.Substring(0, op.Length - 2) + "-";
                }
                else if (op.EndsWith("+"))
                    op = op.Substring(0, op.Length - 1);
                else
                {
                    return op;
                }
            }
            return op;
        }

        public static double GetValue(string textLevel, string patternItem, List<double> listValue, List<string> listLevel, ref EvalWarningFlags warningFlags)
        {
            MatchCollection matches;
            matches = Regex.Matches(textLevel, patternItem);
           
            double baseValue = 1;
            double value0 = double.NaN, value1 = double.NaN;
            if (matches.Count >= 1)
            {
                if (matches[0].Value.Equals(textLevel))
                {
                    int valIdx;
                    if (int.TryParse(textLevel.Substring(1, textLevel.Length-2),out valIdx))
                    {
                        if (textLevel.StartsWith("{"))
                            return GetValue(listLevel[valIdx], patternItem, listValue, listLevel, ref warningFlags);
                        else
                            return listValue[valIdx];
                    }
                    else
                    {
                        throw new Exception("ERROR Next idx@ : " + textLevel);
                    }
                }
                value0 = GetValue(matches[0].Value, patternItem, listValue, listLevel, ref warningFlags);

                if (matches[0].Index != 0)
                {
                    string op0 = textLevel.Substring(0, matches[0].Index);
                    op0 = FixAddAndSub(op0);
                    while (op0.Length != 0 && (op0[0] == '-' || op0[0] == '+'))
                    {
                        if (op0[0] == '-')
                            baseValue *= -1;
                        op0 = op0.Substring(1, op0.Length - 1);
                    }

                    value0 *= baseValue;

                    if (op0.Length != 0)
                    {
                        double value0Tmp;
                        switch (op0)
                        {
                            case "log":
                                value0Tmp = Math.Log(value0);
                                break;
                            case "exp":
                                value0Tmp = Math.Exp(value0);
                                break;
                            case "sqrt":
                                value0Tmp = Math.Sqrt(value0);
                                break;

                            case "cos":
                                value0Tmp = Math.Cos(value0);
                                break;
                            case "cosh":
                                value0Tmp = Math.Cosh(value0);
                                break;
                            case "acos":
                                value0Tmp = Math.Acos(value0);
                                break;

                            case "sin":
                                value0Tmp = Math.Sin(value0);
                                break;
                            case "sinh":
                                value0Tmp = Math.Sinh(value0);
                                break;
                            case "asin":
                                value0Tmp = Math.Asin(value0);
                                break;

                            case "tan":
                                value0Tmp = Math.Tan(value0);
                                break;
                            case "tanh":
                                value0Tmp = Math.Tanh(value0);
                                break;
                            case "atan":
                                value0Tmp = Math.Atan(value0);
                                break;

                            case "int":
                                if (!(value0 >= -9223372036854775808.0   // -2^63
                                     && value0 < 9223372036854775808.0)) // 2^63
                                    throw new OverflowException($"{value0} to int64");
                                value0Tmp = (long)value0;
                                break;
                            default:
                                throw new Exception("ERROR unknown op0@ : " + textLevel + " op:" + op0);
                        }
                        if (double.IsNaN(value0Tmp))
                            throw new ArgumentOutOfRangeException("",$"{op0}({value0})");
                        value0 = value0Tmp;
                    }
                }
                if (matches.Count == 1)
                {
                    if (matches[0].Index + matches[0].Length != textLevel.Length)
                        throw new Exception("ERROR unknown status@ : " + textLevel + " matchcount:" + matches.Count);
                    return value0;
                }
            }

            if (matches.Count == 2)
            {

                if (matches[matches.Count - 1].Index + matches[matches.Count - 1].Length != textLevel.Length)
                    throw new Exception("ERROR unknown status@ : " + textLevel + " matchcount:" + matches.Count);

                int opStart1 = 0, opEnd1 = 0;
                opStart1 = matches[matches.Count - 2].Index + matches[matches.Count - 2].Length;
                opEnd1 = matches[matches.Count - 1].Index;
                value1 = GetValue(matches[1].Value, patternItem, listValue, listLevel, ref warningFlags);

                string op1 = "";
                if (opEnd1 == opStart1)
                    op1 = "+";
                else if (opEnd1 > opStart1)
                {
                    op1 = textLevel.Substring(opStart1, opEnd1 - opStart1);
                    op1 = FixAddAndSub(op1);
                }

                if (op1.Length !=0)
                { 
                    switch (op1)
                    {
                        case "+":
                            return value0 + value1;
                        case "-":
                            return value0 - value1;
                        case "*":
                            return value0 * value1;
                        case "/":
                            if (value1 == 0)
                                throw new DivideByZeroException("ERROR div by zero@ : " + textLevel + " value1:" + matches[1].Value);
                            return value0 / value1;
                        case "pow":
                            return Math.Pow(value0, value1);
                        case "%":
                            return (long)value0 % (long)value1;
                        case "^":
                            return (long)value0 ^ (long)value1;
                        case "|":
                            return (long)value0 | (long)value1;
                        case "&":
                            return (long)value0 & (long)value1;
                        case "<<":
                            if (value1 > Int32.MaxValue)
                                throw new OverflowException($"{value1} to int");
                            else if ((double)((int)value0) != value0 || (double)((int)value1) != value1)
                                warningFlags |= EvalWarningFlags.DoubleToInt;
                            return value1 >= 0 ? (long)value0 << (int)value1 : (long)value0 >> (int)(-value1);
                        case ">>":
                            if (value1 > Int32.MaxValue)
                                throw new OverflowException($"{value1} to int");
                            else if ((double)((int)value0) != value0 || (double)((int)value1) != value1)
                                warningFlags |= EvalWarningFlags.DoubleToInt;
                            return value1 >= 0 ? (long)value0 >> (int)value1 : (long)value0 << (int)(-value1);


                    }
                }
                else
                {
                    throw new Exception("ERROR unknown op1@ : " + textLevel + " opStart:" + opStart1 + " opEnd:" + opEnd1);
                }
            }
            else
            {
                throw new Exception("ERROR too much matches@ : " + textLevel + " matchcount:" + matches.Count);
            }

            throw new Exception("ERROR unknown status@ : " + textLevel + " matchcount:" + matches.Count);
        }

        public enum EvalWarningFlags
        {
            None = 0x0000,
            DoubleToInt = 0x0001
        }

        public static double Eval(string text)
        {
            EvalWarningFlags warningFlags = EvalWarningFlags.None;
            return Eval(text, ref warningFlags);
        }
        public static double Eval(string text,ref EvalWarningFlags warningFlags)
        {
            text = text.Trim().Replace(" ", "");

            string[] specialStringList = { "{", "}", "[", "]" };
            foreach (string x in specialStringList)
            {
                if (text.Contains(x))
                {
                    throw new InvalidOperationException("Contains unsupport char:" + x + " input:"+ text);
                }
            }


            bool isWrongFmtDetect;
            do
            {
                isWrongFmtDetect = false;
                while (isWrongFmtDetect |= text.Contains("--"))
                    text = text.Replace("--", "+");
                while (isWrongFmtDetect |= text.Contains("-+"))
                    text = text.Replace("-+", "-");
                while (isWrongFmtDetect |= text.Contains("+-"))
                    text = text.Replace("+-", "-");
                while (isWrongFmtDetect |= text.Contains("--"))
                    text = text.Replace("--", "+");
                while (isWrongFmtDetect |= text.Contains("++"))
                    text = text.Replace("++", "+");
                while (isWrongFmtDetect |= text.Contains("+-"))
                    text = text.Replace("+-", "-");
            } while (isWrongFmtDetect);


            MatchCollection matches;
            int rootIdx = 0;
            List<double> listValue = new List<double>();
            List<string> listLevel = new List<string>();


            //Step1. replace numbers
            //matches = Regex.Matches(text, "exp|e");
            //var matchList = matches.Cast<Match>().Where(x => x.Value.ToLower().Equals("e")).ToList<Match>();

            ////matches = Regex.Matches(text, "((0x|0b|-|\\+)?[\\da-fA-F\\.]+b?|pi|conste)");//\(([^\(\)]*)\)
            //matches = Regex.Matches(text, "(0x)[\\da-fA-F]+|(0b)[01]+|[01]+b|pi|conste|(-|\\+)?[\\d.]+");//\(([^\(\)]*)\)
            //matchList.AddRange(matches.Cast<Match>().ToList());

            matches = Regex.Matches(text, "(0x)[\\da-fA-F]+|(0b)[01]+|[01]+b|pi|conste|(-|\\+)?[\\d.]+|exp|e");//\(([^\(\)]*)\)
            var matchList = matches.Cast<Match>().Where(x => !x.Value.ToLower().Equals("exp")).ToList<Match>();

            string textNoNum = ReplaceWithIndex(text, matchList, true, listValue.Count, "[", "]");
            foreach (Match m in matchList)
            {
                double val = StrToValue(m.Value);
                if (Double.IsNaN(val))
                {
                    throw new Exception ("ERROR [" + m.Value + "] can not convert to number....");
                }
                listValue.Add(val);
            }

            //Step2. Detect parentheses pairs
            string textLevel = textNoNum;
            Console.WriteLine(textLevel);
            int level = 0;
            
            do
            {
                matches = Regex.Matches(textLevel, "\\(([^\\(\\)]*)\\)");

                textLevel = ReplaceWithIndex(textLevel, matches, true, listLevel.Count, "{", "}");
                foreach (Match m in matches)
                {
                    listLevel.Add(m.Value.Substring(1, m.Value.Length-2));
                }
                Console.WriteLine(level + ": " + textLevel);
                level++;
            } while (matches.Count != 0);

            rootIdx = listLevel.Count;
            listLevel.Add(textLevel);


            Console.WriteLine("Levelv0 (root:" + rootIdx + "):");
            for (int i = 0; i < listLevel.Count; i++)
                Console.WriteLine(i + ": " + listLevel[i]);


            //Step3. Detect the supported operators
            string patternItem = "[\\[\\{]\\d+[\\]\\}]";//\\[\\d+\\]
            string patternOneSideOp = "log|exp" + "|[a]?cos[h]?|[a]?tan[h]?|[a]?sin[h]?" + "|sqrt|int";
            for (int l = 0; l < listLevel.Count; l++)
            {

                //matches = Regex.Matches(listLevel[l], patternItem);
                //if (matches.Count < 2)
                //    continue;
                matches = Regex.Matches(listLevel[l], patternOneSideOp);
                if (matches.Count <= 1)
                    continue;

                matches = Regex.Matches(listLevel[l], "("+patternOneSideOp+")" + patternItem);
                if (matches.Count == 0)
                    break;
                
                string textTmp = ReplaceWithIndex(listLevel[l], matches, true, listLevel.Count, "{", "}");
                listLevel[l] = textTmp;
                foreach (Match m in matches)
                {
                    listLevel.Add(m.Value);
                }
                l--;
            }
            
            string [] patternBaseList = new string[] { "(\\*|\\/|pow|%|>>|<<|\\^|\\||\\&)+" + patternItem, "[\\+\\-]+"+ patternItem };
            for (int p=0;p< patternBaseList.Length;p++)
            {
                string patternBase = patternBaseList[p];
                for (int l = 0; l < listLevel.Count; l++)
                {
                    matches = Regex.Matches(listLevel[l], patternItem);
                    if (matches.Count <= 2)
                        continue;

                    matches = Regex.Matches(listLevel[l], patternItem + "(" + patternBase + ")+");

                    if (matches.Count == 1 && matches[0].Value.Equals(listLevel[l]))
                    {
                        matches = Regex.Matches(listLevel[l], patternBase);
                        PrintMatches(matches, listLevel[l]);
                        if (matches.Count > 1)
                        {
                            string valEnd = matches[matches.Count - 1].Value;
                            listLevel.Add(listLevel[l].Replace(valEnd, ""));
                            listLevel[l] = "{" + (listLevel.Count - 1) + "}" + valEnd;
                            Console.WriteLine(p + "-" + l + " " + listLevel[l]);
                        }
                    }
                    else
                    {
                        string textTmp = ReplaceWithIndex(listLevel[l], matches, true, listLevel.Count, "{", "}");
                        listLevel[l] = textTmp;
                        foreach (Match m in matches)
                        {
                            listLevel.Add(m.Value);
                        }
                    }
                }
            }


            double result = double.NaN;
            result = GetValue(listLevel[rootIdx], patternItem, listValue, listLevel,ref warningFlags);


            Console.WriteLine("Value:");
            for (int i = 0; i < listValue.Count; i++)
                Console.WriteLine(i + ": " + listValue[i]);

            Console.WriteLine("Levelv1 (root:" + rootIdx+ "):");
            for (int i = 0; i < listLevel.Count; i++)
                Console.WriteLine(i + ": " + listLevel[i]);

            Console.WriteLine("Result : " + result);
            //PrintMatches(matches, text);

            return result;
        }
    }
}
