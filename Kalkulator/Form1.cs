using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kalkulator
{
    public partial class Calculator : Form
    {
        public enum CalcKeyFunction
        {
            Numeric = 1,
            Char = 2,
            Option = 3
        }

        public enum CalcLineFunc
        {
            Syntax = 1,
            Memory = 2, 
            Result = 3
        }

        public class CalcKey
        {
            public CalcKeyFunction Func { get; set; }
            public string Char { get; set; }
        }

        public List<CalcKey> Keys { get; set; }

        public double CalcPart1 { get; set; }
        public double CalcPart2 { get; set; }
        public string CalcChar { get; set; }

        public bool ClearLine { get; set; }

        public double Memory { get; set; }


        public Calculator()
        {
            InitializeComponent();

            BuildCalcDict();
        }

        public void BuildCalcDict()
        {
            Keys = new List<CalcKey>();

            foreach (var ctrl in this.Controls)
            {
                if (ctrl is Button)
                {
                    var btn = (Button)ctrl;

                    int test;
                    char test2;

                    if (int.TryParse(btn.Text, out test))
                    {
                        Keys.Add(new CalcKey() { Char = test.ToString(), Func = CalcKeyFunction.Numeric });

                        btn.Click += Numeric_Click;
                        btn.KeyDown += Calculator_KeyDown;
                    }
                    else if ((char.TryParse(btn.Text, out test2) 
                                && btn.Tag != null 
                                && btn.Tag.ToString() != "Option")
                                || btn.Text == "%")
                    {
                        Keys.Add(new CalcKey() { Char = test2.ToString(), Func = CalcKeyFunction.Char });

                        btn.Click += Char_Click;
                        btn.KeyDown += Calculator_KeyDown;
                    }
                    else
                    {
                        Keys.Add(new CalcKey() { Char = btn.Text, Func = CalcKeyFunction.Option });

                        btn.Click += Option_Click;
                        btn.KeyDown += Calculator_KeyDown;
                    }
                }

                Keys = Keys.OrderBy(x => x.Char).ToList();
            }
        }

        private string[] GetLines()
        {
            if (tbxResult.Lines.Count() < 3)
            {
                string[] lines = new string[3] { "", "", "" };

                tbxResult.Clear();
                tbxResult.Lines = lines;
            }

            return tbxResult.Text.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
        }

        private string GetLine(CalcLineFunc line)
        {
            var result = GetLines();

            return result[(int)line-1];
        }

        private void SetLine(CalcLineFunc line, string sLine)
        {
            var result = GetLines();

            if ((int)line == 1)
                result[0] = sLine;
            else if ((int)line == 2)
            {
                for (int i = sLine.Length; i < 22; i++)
                    sLine += "  ";

                result[1] = sLine;
            }
            else if ((int)line == 3)
                result[2] = sLine;

            tbxResult.Text = String.Join(Environment.NewLine, result);
        }

        private void AddToLine(CalcLineFunc line, string sLine)
        {
            var result = GetLines();

            if ((int)line == 1)
            {
                result[0] += sLine;

                if (result[0].Length > 22)
                {
                    result[0] = "<<" + result[0].Substring(result[0].Length - 20);
                }
            }
            else if ((int)line == 2)
            {
                result[1] += sLine;
            }
            else if ((int)line == 3 && result[2].Length <= 16)
            {
                if(result[2].Length == 1 && result[2] == "0")
                    result[2] = sLine;
                else
                    result[2] += sLine;
            }

            tbxResult.Text = String.Join(Environment.NewLine, result);
        }

        private void RemoveLastFromLine(CalcLineFunc line)
        {
            var result = GetLines();

            if ((int)line == 1 && result[0].Length > 0)
            {
                result[0] = result[0].Substring(0, result[0].Length - 1).ToString();
            }
            else if((int)line == 2)
            {

            }
            else if ((int)line == 3)
            {
                if (result[2].Length > 1)
                    result[2] = result[2].Substring(0, result[2].Length - 1).ToString();
                else
                    result[2] = "0";
            }

            tbxResult.Text = String.Join(Environment.NewLine, result);
        }

        private void Numeric_Click(object sender, EventArgs e)
        {
            bool IsMouse = (e is System.Windows.Forms.MouseEventArgs);

            if(!IsMouse)
            {
                KeyPress_Event("Return");
            }

            KeyPress_Event(((Button)sender).Text);
        }

        private void Char_Click(object sender, EventArgs e)
        {
            bool IsMouse = (e is System.Windows.Forms.MouseEventArgs);

            if (!IsMouse)
            {
                KeyPress_Event("Return");
            }

            KeyPress_Event(((Button)sender).Text);
        }

        private void Option_Click(object sender, EventArgs e)
        {
            bool IsMouse = (e is System.Windows.Forms.MouseEventArgs);

            if (!IsMouse)
            {
                KeyPress_Event("Return");
            }

            KeyPress_Event(((Button)sender).Text);
        }

        private void tbxResult_KeyDown(object sender, KeyEventArgs e)
        {
            Calculator_KeyDown(sender, e);
        }

        private void Calculator_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;

            KeyPress_Event(e.KeyData.ToString());
        }

        private void KeyPress_Event(string data)
        {
            var key = GetCalcKey(data);

            if (key != null && key is CalcKey)
            {
                CalcKeyPress(key);
            }
        }

        private void CalcKeyPress(CalcKey key)
        {
            switch (key.Func)
            {
                case CalcKeyFunction.Numeric:

                    if (ClearLine)
                    {
                        ClearLine = false;
                        SetLine(CalcLineFunc.Result, "");
                    }

                    AddToLine(CalcLineFunc.Result, key.Char);

                    break;

                case CalcKeyFunction.Char:

                    if (key.Char == ",")
                    {
                        AddToLine(CalcLineFunc.Result, key.Char);
                        break;
                    }

                    if (CalcPart1 == 0)
                    {
                        double c1;
                        if (double.TryParse(GetLine(CalcLineFunc.Result), out c1))
                        {
                            CalcPart1 = c1;

                            CalcChar = key.Char;

                            SetLine(CalcLineFunc.Syntax, GetLine(CalcLineFunc.Result));
                            AddToLine(CalcLineFunc.Syntax, key.Char);

                            ClearLine = true;
                        }
                    }
                    else
                    {
                        double c2;
                        if (double.TryParse(GetLine(CalcLineFunc.Result), out c2))
                        {
                            if (key.Char == "%")
                            {
                                CalcPart2 = CalcPart1 * c2 / 100;

                                SetLine(CalcLineFunc.Result, CalcPart2.ToString());

                                AddToLine(CalcLineFunc.Syntax, CalcPart2.ToString());
                            }
                            else
                            {

                                CalcPart2 = c2;

                                double result = CalcResult();

                                CalcChar = key.Char;

                                AddToLine(CalcLineFunc.Syntax, GetLine(CalcLineFunc.Result));

                                if (key.Char != "=")
                                {
                                    AddToLine(CalcLineFunc.Syntax, key.Char);
                                }

                                SetLine(CalcLineFunc.Result, result.ToString());

                                CalcPart1 = result;

                                CalcPart2 = 0;
                            }

                            ClearLine = true;
                        }
                    }
                    break;

                case CalcKeyFunction.Option:

                    if (key.Char == "<")
                    {
                        RemoveLastFromLine(CalcLineFunc.Result);
                        break;
                    }

                    if(key.Char == "+/-")
                    {
                        string value = GetLine(CalcLineFunc.Result);

                        double d;
                        if (double.TryParse(value, out d))
                        {
                            d = -d;
                            SetLine(CalcLineFunc.Result, d.ToString());
                        }
                        break;
                    }

                    if(key.Char == "sqrt(x)")
                    {
                        string value = GetLine(CalcLineFunc.Result);

                        double d;
                        if (double.TryParse(value, out d))
                        {
                            SetLine(CalcLineFunc.Syntax, "sqrt(" + d.ToString() + ")");
                            d = Math.Sqrt(d);
                            SetLine(CalcLineFunc.Result, d.ToString());
                        }
                        break;
                    }

                    if (key.Char == "1/x")
                    {
                        string value = GetLine(CalcLineFunc.Result);

                        double d;
                        if (double.TryParse(value, out d))
                        {
                            SetLine(CalcLineFunc.Syntax, "reciproc(" + d.ToString() + ")");
                            d = 1/d;
                            SetLine(CalcLineFunc.Result, d.ToString());
                        }
                        break;
                    }

                    double mem;

                    switch(key.Char)
                    {
                        case "MS":

                            if(double.TryParse(GetLine(CalcLineFunc.Result), out mem))
                            {
                                Memory = mem;
                            }

                            break;

                        case "MR":

                            SetLine(CalcLineFunc.Result, Memory.ToString());

                            break;

                        case "MC":

                            Memory = 0;

                            break;

                        case "M+":

                            if (double.TryParse(GetLine(CalcLineFunc.Result), out mem))
                            {
                                Memory += mem;
                            }

                            break;

                        case "M-":

                            if (double.TryParse(GetLine(CalcLineFunc.Result), out mem))
                            {
                                Memory -= mem;
                            }

                            break;

                        case "C":
                            
                            CalcPart1 = 0;
                            CalcPart2 = 0;
                            CalcChar = "";
                            SetLine(CalcLineFunc.Syntax, "");
                            SetLine(CalcLineFunc.Result, "");

                            break;

                        case "CE":

                            SetLine(CalcLineFunc.Result, "");

                            break;
                    }

                    SetLine(CalcLineFunc.Memory, Memory != 0 ? "M" : "");

                    break;
            }
        }

        private double CalcResult()
        {
            double result = 0;

            switch (CalcChar)
            {
                case "+":
                    result = (CalcPart1 + CalcPart2);
                    break;

                case "-":
                    result = (CalcPart1 - CalcPart2);
                    break;

                case "*":
                    result = (CalcPart1 * CalcPart2);
                    break;

                case "/":
                    if (CalcPart2 > 0)
                        result = (CalcPart1 / CalcPart2);
                    else
                    {
                        SetLine(CalcLineFunc.Result, "Błąd !");
                        ClearLine = true;
                    }
                    break;
            }

            return result;
        }

        private CalcKey GetCalcKey(string keyData)
        {
            keyData = keyData.ToString();

            keyData = keyData.Replace("Add", "+")
                            .Replace("Subtrack", "-")
                            .Replace("Divide", "/")
                            .Replace("Multiply", "*")
                            .Replace("Return", "=")
                            .Replace("Back", "<")
                            .Replace("Decimal", ",")
                            .Replace("NumPad", "")
                            .Replace("D", "")
                            .Replace("Escape", "C");

            return Keys.FirstOrDefault(x => x.Char == keyData);
        }
    }
}
