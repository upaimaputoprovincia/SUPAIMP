using supai_mp.Models;

namespace supai_mp.Helpers
{
    public static class OrdenacaoFuncionarios
    {
        /// <summary>
        /// Ordem das chefias das secções.
        /// Quanto menor o número, maior a prioridade.
        /// </summary>
        public static int OrdemChefia(string? funcao)
        {
            if (string.IsNullOrWhiteSpace(funcao))
                return 9999;

            var valor = funcao.Trim();

            return valor switch
            {
                "ChefeDeCMD" => 1,
                "ChefeDeOP" => 2,
                "ChefeDeDEP" => 3,
                "ChefeDePEAC" => 4,
                "ChefeDeSP" => 5,
                "ChefeDePO" => 6,
                "ChefeDeLogistica" => 7,
                "ChefeDeGPF" => 8,
                "ChefeDeSECRE" => 9,
                "ChefeDeIO" => 10,
                "ChefeDeII" => 11,

                // Compatibilidade caso algum registo antigo
                // esteja gravado com o Display Name.
                "COMANDANTE DA SUPAI_MP" => 1,
                "CHEFE DE SEÇÃO DAS OPERAÇÕES" => 2,
                "CHEFE DE SEÇÃO DE DOUTRINA E ÉTICA POLICIAL" => 3,
                "CHEFE DE SEÇÃO DE PEAC" => 4,
                "CHEFE DE SEÇÃO DE SEGURANÇA PESSOAL" => 5,
                "CHEFE DE SEÇÃO DE PROTECÇÃO DE OBJECTO" => 6,
                "CHEFE DE SEÇÃO DE LOGÍSTICA E FINANÇAS" => 7,
                "CHEFE DE SEÇÃO DE GESTÃO DE PESSOAL E FORMAÇÃO" => 8,
                "CHEFE DE SEÇÃO DA SECRETARIA" => 9,
                "CHEFE DE SEÇÃO DE INFORMAÇÃO OPERATIVA" => 10,
                "CHEFE DE SEÇÃO DE INFORMAÇÃO INTERNA" => 11,

                _ => 9999
            };
        }

        /// <summary>
        /// Ordem hierárquica das categorias da Polícia.
        /// </summary>
        public static int OrdemCategoria(Categoria categoria)
        {
            return categoria switch
            {
                Categoria.IPG => 1,
                Categoria.COM => 2,
                Categoria.PAC => 3,
                Categoria.AJC => 4,
                Categoria.SPP => 5,
                Categoria.SUP => 6,
                Categoria.ASP => 7,
                Categoria.INP => 8,
                Categoria.INS => 9,
                Categoria.SUB => 10,
                Categoria.SAP => 11,
                Categoria.SAR => 12,
                Categoria.PC => 13,
                Categoria.SC => 14,
                Categoria.GUA => 15,

                _ => 9999
            };
        }

        /// <summary>
        /// Verifica se uma função é uma das chefias principais
        /// da hierarquia das secções.
        /// </summary>
        public static bool EhChefiaPrincipal(string? funcao)
        {
            return OrdemChefia(funcao) < 9999;
        }
    }
}