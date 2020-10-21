using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PDV_WPF.Funcoes
{
    public static class Extensions
    {
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public static ImageSource ToImageSource(this Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        #region STRING
        /// <summary>
        /// Converte uma string de valor monetário em seu valor decimal
        /// </summary>
        /// <param name="currencyString">String, com casas decimais e símbolo monetário correspondente à cultura informada</param>
        /// <param name="format">Cultura a ser utilizada na conversão da string</param>
        /// <returns></returns>
        public static string Truncate(this string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return str.Length <= maxLength ? str : str.Substring(0, maxLength);
        }
        public static decimal ExtrairValorFromCurrencyString(this string currencyString, IFormatProvider format)
        {
            currencyString = currencyString.ToUpper();
            currencyString = currencyString.Trim();
            decimal.TryParse(currencyString, System.Globalization.NumberStyles.AllowCurrencySymbol, format, out decimal retorno);
            return retorno;
        }
        public static string GetHashCode(this string inputString)
        {
            // Todos os disposables objects devem ser desfeitos apropriadamente com ".Dispose()"
            // ou aplicando o using, como está abaixo.
            // A não execução desse método pode causar erros aleatórios no sistema, como memory leak.
            // Fonte: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-statement
            using MD5 md5Hash = MD5.Create();
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(inputString));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }//Transforma uma string em has MD5.
        public static bool IsNumbersOnly(this string str)
        {
            if (str.Length == 0) return false;
            foreach (char c in str)
            {
                if (!Char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }//Checa se a string possui apenas dígitos.
        public static string Safestring(this string vDado)
        {
            string vResult = string.Empty;
            if (!string.IsNullOrWhiteSpace(vDado))
            {
                vResult = vDado.ToString();
            }
            return vResult;
        }
        public static decimal Safedecimal(this string vDado)
        {
            decimal vResult = 0;
            if (!string.IsNullOrWhiteSpace(vDado))
            {
                //if (vDado != "NaN") //TODO: Não é necessário, pois a conversão a seguir já pode estourar e resultar em zero.
                //{
                try
                {
                    vResult = Convert.ToDecimal(vDado);
                }
                catch
                {
                    vResult = 0;
                }
                //}
            }
            return vResult;
        }
        public static bool Safebool(this string vDado)
        {
            bool vResult = false;

            if (!string.IsNullOrWhiteSpace(vDado))
            {
                try
                {
                    vResult = Convert.ToBoolean(vDado);
                }
                catch (Exception)
                {
                    vResult = false;
                }
            }

            return vResult;
        }
        public static DateTime Safedate(this string vDado)
        {
            DateTime vResult = DateTime.MinValue;
            if (!string.IsNullOrWhiteSpace(vDado))
            {
                try
                {
                    vResult = Convert.ToDateTime(vDado);
                }
                catch
                {
                    vResult = DateTime.MinValue;
                }
            }
            return vResult;
        }
        public static double Safedouble(this string vDado)
        {
            double vResult = 0;
            if (!string.IsNullOrWhiteSpace(vDado))
            {
                //if (vDado != "NaN") //TODO: Não é necessário, pois a conversão a seguir já pode estourar e resultar em zero.
                //{
                try
                {
                    vResult = Convert.ToDouble(vDado);
                }
                catch
                {
                    vResult = 0;
                }
                //}
            }
            return vResult;
        }
        public static byte Safebyte(this string vDado)
        {
            byte vResult = 0;
            if (!string.IsNullOrWhiteSpace(vDado))
            {
                //if (vDado != "NaN") //TODO: Não é necessário, pois a conversão a seguir já pode estourar e resultar em zero.
                //{
                try
                {
                    vResult = Convert.ToByte(vDado);
                }
                catch
                {
                    vResult = 0;
                }
                //}
            }
            return vResult;
        }
        public static short Safeshort(this string vDado)
        {
            short vResult = 0;
            if (!string.IsNullOrWhiteSpace(vDado))
            {
                //if (vDado != "NaN") //TODO: Não é necessário, pois a conversão a seguir já pode estourar e resultar em zero.
                //{
                try
                {
                    vResult = Convert.ToInt16(vDado);
                }
                catch
                {
                    vResult = 0;
                }
                //}
            }
            return vResult;
        }
        public static int Safeint(this string vDado)
        {
            int vResult = 0;
            if (!string.IsNullOrWhiteSpace(vDado))
            {
                //if (vDado != "NaN") //TODO: Não é necessário, pois a conversão a seguir já pode estourar e resultar em zero.
                //{
                try
                {
                    vResult = Convert.ToInt32(vDado);
                }
                catch
                {
                    vResult = 0;
                }
                //}
            }
            return vResult;
        }
        public static long Safelong(this string vDado)
        {
            long vResult = 0;
            if (!string.IsNullOrWhiteSpace(vDado))
            {
                //if (vDado != "NaN") //TODO: Não é necessário, pois a conversão a seguir já pode estourar e resultar em zero.
                //{
                try
                {
                    vResult = Convert.ToInt64(vDado);
                }
                catch
                {
                    vResult = 0;
                }
                //}
            }
            return vResult;
        }
        public static List<string> SplitNChars(this string s, int partLength)
        {
            List<string> final = new List<string>();
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            if (partLength <= 0)
            {
                throw new ArgumentException("partLegth deve ser positivo.", "partLength");
            }

            for (var i = 0; i < s.Length; i += partLength)
            {
                final.Add(s.Substring(i, Math.Min(partLength, s.Length - i)));
            }

            return final;
        }
        public static string RemoveDarumaFormatting(this string s)
        {
            return s.
                Replace("<S>", String.Empty).Replace("</S>", String.Empty).
                Replace("<N>", String.Empty).Replace("</N>", String.Empty).
                Replace("<W>", String.Empty).Replace("</W>", String.Empty).
                Replace("<C>", String.Empty).Replace("</C>", String.Empty).
                Replace("<H>", String.Empty).Replace("</H>", String.Empty).
                Replace("<XL>", String.Empty).Replace("</XL>", String.Empty);
        }
        public static string TiraPont(this string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s)
            {
                if (!char.IsPunctuation(c))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }//Tira pontuação da string.
        public static string TruncateLongString(this string str)
        {
            return str.Length <= 27 ? str : str.Remove(27);
        }//Trunca strings acima de 27 chars.
        public static string Trunca(this string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            return str.Length <= maxLength ? str : str.Substring(0, maxLength);
        }//Trunca strings acima de maxLength.

        #endregion

        #region OBJECT
        public static string Safestring(this object vDado)
        {
            string vResult = string.Empty;
            if (vDado != null)
            {
                if (!string.IsNullOrWhiteSpace(vDado.ToString()))
                {
                    vResult = vDado.ToString();
                }
            }
            return vResult;
        }
        public static decimal Safedecimal(this object vDado)
        {
            decimal vResult = 0;

            if (vDado == null)
            {
                return vResult;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(vDado.ToString()))
                {
                    return vResult;
                }
                //if ((string)vDado == "NaN") return vResult; //TODO: Não é necessário, pois a conversão a seguir já pode estourar e resultar em zero.

                vResult = Convert.ToDecimal(vDado);
            }
            catch
            {
                vResult = 0;
            }

            return vResult;
        }
        public static double Safedouble(this object vDado)
        {
            double vResult = 0;

            if (vDado == null)
            {
                return vResult;
            }

            if (string.IsNullOrWhiteSpace(vDado.ToString()))
            {
                return vResult;
            }
            //if ((string)vDado == "NaN") return vResult; //TODO: Não é necessário, pois a conversão a seguir já pode estourar e resultar em zero.

            try
            {
                vResult = Convert.ToDouble(vDado);
            }
            catch
            {
                vResult = 0;
            }

            return vResult;
        }
        public static short Safeshort(this object vDado)
        {
            short vResult = 0;

            if (vDado == null)
            {
                return vResult;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(vDado.ToString()))
                {
                    return vResult;
                }
                //if ((string)vDado == "NaN") return vResult; //TODO: Não é necessário, pois a conversão a seguir já pode estourar e resultar em zero.

                vResult = Convert.ToInt16(vDado);
            }
            catch
            {
                vResult = 0;
            }

            return vResult;
        }
        public static int Safeint(this object objeto)
        {
            int vResult = 0;

            if (objeto != null && !string.IsNullOrWhiteSpace(objeto.ToString()))
            {
                //if (objeto.ToString() != "NaN") //TODO: Não é necessário, pois a conversão a seguir já pode estourar e resultar em zero.
                //{
                try
                {
                    vResult = Convert.ToInt32(objeto);
                }
                catch
                {
                    vResult = 0;
                }
                //}
            }
            return vResult;
        }
        public static long Safelong(this object objeto)
        {
            long vResult = 0;

            if (objeto != null && !string.IsNullOrWhiteSpace(objeto.ToString()))
            {
                //if (objeto.ToString() != "NaN") //TODO: Não é necessário, pois a conversão a seguir já pode estourar e resultar em zero.
                //{
                try
                {
                    vResult = Convert.ToInt64(objeto);
                }
                catch
                {
                    vResult = 0;
                }
                //}
            }
            return vResult;
        }
        #endregion

        #region DECIMAL
        public static decimal Truncate(this decimal number, int digits)
        {
            decimal stepper = (decimal)(Math.Pow(10.0, digits));
            int temp = (int)(stepper * number);
            return temp / stepper;
        }
        private static decimal ExtraiDigitoSignificativo(decimal valor)
        {
            return (valor - Math.Truncate(valor)) * 10;
        }
        public static decimal RoundABNT(this decimal value, int places = 2)
        {

            decimal a, b, c, d;
            decimal algAManter, algAAvaliar, algsADireita;

            if (places != 3)
            {
                a = ExtraiDigitoSignificativo(value);
                b = ExtraiDigitoSignificativo(a);
                c = ExtraiDigitoSignificativo(b);

                algAAvaliar = Math.Truncate(c);
                algsADireita = c - Math.Truncate(c);
                algAManter = Math.Truncate(b);
                places = 2;
            }
            else
            {
                a = ExtraiDigitoSignificativo(value);
                b = ExtraiDigitoSignificativo(a);
                c = ExtraiDigitoSignificativo(b);
                d = ExtraiDigitoSignificativo(c);

                algAAvaliar = Math.Truncate(d);
                algsADireita = d - Math.Truncate(d);
                algAManter = Math.Truncate(c);
            }

            if (algAAvaliar < 5)
            {
                return Math.Truncate(value * (decimal)Math.Pow(10, places)) / (decimal)Math.Pow(10, places);
            }
            if (algAAvaliar > 5)
            {
                return (Math.Truncate((value * (decimal)Math.Pow(10, places)) + 1) / (decimal)Math.Pow(10, places));
            }
            if (algAAvaliar == 5)
            {
                if (algsADireita != 0)
                {
                    return (Math.Truncate((value * (decimal)Math.Pow(10, places)) + 1) / (decimal)Math.Pow(10, places));
                }
                if (algsADireita == 0)
                {
                    if (algAManter % 2 == 1)
                    {
                        return (Math.Truncate((value * (decimal)Math.Pow(10, places)) + 1) / (decimal)Math.Pow(10, places));
                    }
                    if (algAManter % 2 == 0)
                    {
                        return Math.Truncate(value * (decimal)Math.Pow(10, places)) / (decimal)Math.Pow(10, places);
                    }
                }
            }
            return value;
        }
        /// <summary>
        /// Retorna verdadeiro se o número está entre os limites estabelecidos.
        /// </summary>
        /// <param name="value">Valor a ser comparado</param>
        /// <param name="lowerValue">Limite inferior</param>
        /// <param name="higherValue">Limite superior</param>
        /// <param name="inclusive">Permitir os limites inclusive</param>
        /// <returns></returns>
        public static bool IsBetween(this decimal value, decimal lowerValue, decimal higherValue, bool inclusive = true)
        {
            if (inclusive)
            {
                if (value <= higherValue && value >= lowerValue) return true;
                else return false;
            }
            else
            {
                if (value < higherValue && value > lowerValue) return true;
                else return false;
            }
        }
        #endregion

        #region DOUBLE
        public static double Safedouble(this double vDado)
        {
            double vResult = 0;
            try
            {
                if (!string.IsNullOrWhiteSpace(Convert.ToString(Convert.ToDouble(vDado))))
                {
                    vResult = vDado;
                }
            }
            catch
            {
                vResult = 0;
            }
            return vResult;
        }
        /// <summary>
        /// Retorna verdadeiro se o número está entre os limites estabelecidos.
        /// </summary>
        /// <param name="value">Valor a ser comparado</param>
        /// <param name="lowerValue">Limite inferior</param>
        /// <param name="higherValue">Limite superior</param>
        /// <param name="inclusive">Permitir os limites inclusive</param>
        /// <returns></returns>
        public static bool IsBetween(this double value, double lowerValue, double higherValue, bool inclusive = true)
        {
            if (inclusive)
            {
                if (value <= higherValue && value >= lowerValue) return true;
                else return false;
            }
            else
            {
                if (value < higherValue && value > lowerValue) return true;
                else return false;
            }
        }

        #endregion

        #region TASK
        public static Task<bool?> ShowDialogAsync(this Window self)
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            TaskCompletionSource<bool?> completion = new TaskCompletionSource<bool?>();
            self.Dispatcher.BeginInvoke(new Action(() => completion.SetResult(self.ShowDialog())));

            return completion.Task;
        }
        #endregion

        #region DATETIME
        public static DateTime Safedate(this DateTime vDado)
        {
            DateTime vResult = DateTime.Now.Date;
            if (!string.IsNullOrWhiteSpace(Convert.ToString(vDado)))
            {
                vResult = vDado;
            }
            return vResult;
        }
        #endregion

        #region DICTIONARY
        public static Dictionary<string, decimal> AddRange(this Dictionary<string, decimal> dict, params (string, decimal)[] parametros)
        {
            foreach (var item in parametros)
            {
                dict.Add(item.Item1, item.Item2);
            }
            return dict;
        }
        public static Dictionary<int, string> AddRange(this Dictionary<int, string> dict, params (int, string)[] parametros)
        {
            foreach (var item in parametros)
            {
                dict.Add(item.Item1, item.Item2);
            }
            return dict;
        }
        #endregion

        #region INT
        /// <summary>
        /// Retorna verdadeiro se o número está entre os limites estabelecidos.
        /// </summary>
        /// <param name="value">Valor a ser comparado</param>
        /// <param name="lowerValue">Limite inferior</param>
        /// <param name="higherValue">Limite superior</param>
        /// <param name="inclusive">Permitir os limites inclusive</param>
        /// <returns></returns>
        public static bool IsBetween(this int value, int lowerValue, int higherValue, bool inclusive = true)
        {
            if (inclusive)
            {
                if (value <= higherValue && value >= lowerValue) return true;
                else return false;
            }
            else
            {
                if (value < higherValue && value > lowerValue) return true;
                else return false;
            }
        }

        public static bool? ToNullableBool(this int value)
        {
            return value switch
            {
                0 => false,
                1 => true,
                _ => null,
            };
        }

        public static bool ToBool(this int value)
        {
            return value switch
            {
                1 => true,
                _ => false,
            };
        }
        #endregion INT

        #region SHORT
        public static bool? ToNullableBool(this short value)
        {
            return value switch
            {
                0 => false,
                1 => true,
                _ => null,
            };
        }

        public static bool ToBool(this short value)
        {
            return value switch
            {
                1 => true,
                _ => false,
            };
        }
        #endregion SHORT

        #region BOOL
        public static int ToInt(this bool? value)
        {
            return value switch
            {
                true => 1,
                false => 0,
                null => -1,
            };
        }

        public static int ToInt(this bool value)
        {
            return value switch
            {
                true => 1,
                false => 0,
            };
        }

        public static short ToShort(this bool? value)
        {
            return value switch
            {
                true => 1,
                false => 0,
                null => -1,
            };
        }

        public static short ToShort(this bool value)
        {
            return value switch
            {
                true => 1,
                false => 0,
            };
        }
        #endregion BOOL
    }
}
