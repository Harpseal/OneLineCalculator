using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

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

        public class EvalItem
        {
            private double mVal = Double.NaN;
            private string mStr;
            public EvalItem(string text)
            {

            }

            public bool isNumber()
            {
                return !Double.IsNaN(mVal);
            }

        }

        //public class EvalTreeItem
        //{
        //    public EvalTreeItem(List<string> child) { }

        //    public List<string>
        //}
        

        public static string ReplaceWithIndex(string text, MatchCollection matches,
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
                if (text.StartsWith("0x"))
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

        public static double GetValue(string textLevel, string patternItem, List<double> listValue, List<string> listLevel)
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
                            return GetValue(listLevel[valIdx], patternItem, listValue, listLevel);
                        else
                            return listValue[valIdx];
                    }
                    else
                    {
                        throw new Exception("ERROR Next idx@ : " + textLevel);
                    }
                }
                value0 = GetValue(matches[0].Value, patternItem, listValue, listLevel);

                if (matches[0].Index != 0)
                {
                    string op0 = textLevel.Substring(0, matches[0].Index);
                    op0 = FixAddAndSub(op0);
                    if (op0.Equals("-"))
                        baseValue = -1;
                    else if (!op0.Equals("+"))
                        throw new Exception("ERROR unknown op0@ : " + textLevel + " op:" + op0);

                    value0 *= baseValue;
                }
                if (matches.Count == 1)
                    return value0;
            }

            if (matches.Count == 2)
            {
                int opStart1 = 0, opEnd1 = 0;
                opStart1 = matches[matches.Count - 2].Index + matches[matches.Count - 2].Length;
                opEnd1 = matches[matches.Count - 1].Index;
                value1 = GetValue(matches[1].Value, patternItem, listValue, listLevel);

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
                                throw new OverflowException("Overflow at converting double(" + value1 + ") to int32(max:" + Int32.MaxValue + ")");
                            return value1 >= 0 ? (long)value0 << (int)value1 : (long)value0 >> (int)(-value1);
                        case ">>":
                            if (value1 > Int32.MaxValue)
                                throw new OverflowException("Overflow at converting double(" + value1 + ") to int32(max:" + Int32.MaxValue + ")");
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


        public static double Eval(string text)
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



            //Console.WriteLine("fix +-: " + text);
            //return 0;

            MatchCollection matches;
            int rootIdx = 0;
            List<double> listValue = new List<double>();
            List<string> listLevel = new List<string>();

            matches = Regex.Matches(text, "(0x|0b|-|\\+)?[\\da-fA-F\\.]+b?");//\(([^\(\)]*)\)

            string textNoNum = ReplaceWithIndex(text, matches, true, listValue.Count, "[", "]");
            foreach (Match m in matches)
            {
                Console.WriteLine(m.Value);
                double val = StrToValue(m.Value);
                if (Double.IsNaN(val))
                {
                    throw new Exception ("ERROR [" + m.Value + "] can not convert to number....");
                }
                listValue.Add(val);
            }
            //PrintMatches(matches);
            //Console.WriteLine(text);
            //Console.WriteLine(textNoNum);
            //return 0;

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


            string patternItem = "[\\[\\{]\\d+[\\]\\}]";//\\[\\d+\\]
            string [] patternBaseList = new string[] { "(\\*|\\/|pow|%|>>|<<|\\^|\\||\\&)+"+ patternItem, "[\\+\\-]+"+ patternItem };
            //foreach (string patternBase in patternBaseList)
            for (int p=0;p< patternBaseList.Length;p++)
            {
                string patternBase = patternBaseList[p];
                for (int l = 0; l < listLevel.Count; l++)
                {
                    //\\[\\d+\\]((\\+|\\-|\\*|\\/|mod|%)+\\[\\d+\\])+
                    //\\[\\d+\\]((\\*|\\/|mod|%)+\\[\\d+\\])+

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

                Console.WriteLine("Levelv0-" + p + " (root:" + rootIdx + "):");
                for (int i = 0; i < listLevel.Count; i++)
                    Console.WriteLine(i + ": " + listLevel[i]);

            }


            double result = double.NaN;
            //try {
                result = GetValue(listLevel[rootIdx], patternItem, listValue, listLevel);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}


            


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
