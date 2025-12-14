using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.DTOs
{
    public class ResultadoPaginado<T>
    {
        /// <summary>
        /// A lista de itens para a página atual.
        /// </summary>
        public List<T> Itens { get; set; } = new List<T>();

        /// <summary>
        /// A quantidade total de itens que correspondem ao filtro no banco de dados.
        /// </summary>
        public int TotalItens { get; set; }
    }
}
