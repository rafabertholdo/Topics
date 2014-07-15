using System;

namespace Topics.Framework.Util
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class EquivalentValue : Attribute
    {
        private string cValue;
        private StringComparison cStringComparison;
        private int cPriority;

        /// <summary>
        /// Disponibiliza o valor equivalente
        /// </summary>
        public string Value
        {
            get { return cValue; }
            set { cValue = value; }
        }

        /// <summary>
        /// Tipo de comparação a ser realizada na string de valor
        /// </summary>
        public StringComparison StringComparison
        {
            get { return cStringComparison; }
            set { cStringComparison = value; }
        }

        /// <summary>
        /// Ordem de precedência do valor equivalente
        /// </summary>
        public int Priority
        {
            get { return cPriority; }
            set
            {
                if (value < 1)
                    cPriority = 1;
                else
                    cPriority = value;
            }
        }

        /// <summary>
        /// Construtor do atributo
        /// </summary>
        public EquivalentValue(String pValue)
        {
            Value = pValue;
            Priority = 1;
            StringComparison = StringComparison.CurrentCultureIgnoreCase;
        }

        /// <summary>
        /// Construtor do atributo
        /// </summary>
        public EquivalentValue(String pValue, int pPriority)
        {
            Value = pValue;
            Priority = pPriority;
            StringComparison = StringComparison.CurrentCultureIgnoreCase;
        }

        protected EquivalentValue()
        {
        }
    }
}