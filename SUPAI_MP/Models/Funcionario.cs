using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using supai_mp.Models.Organizacao;

namespace supai_mp.Models
{
    public enum EstadoCivil
    {
        Solteiro,
        Casado,
        Divorciado,
        Maritalmente
    }
    public enum NivelAcademico
    {
        Básico,
        Médio,
        Técnico,
        Bacharel,
        Licenciado,
        Mestre
    }
    public enum GrauParentesco
    {
        MÃE,
        PAI,
        ESPOSA,
        ESPOSO,
        IRMÃO,
        IRMÃ,
        FILHO,
        FILHA,
        TIO,
        TIA,
        AVÔ,
        AVÓ,
        SOBRINHO,
        SOBRINHA,
        PRIMO,
        PRIMA
    }
    public enum Categoria
    {
        [Display(Name = "GUARDA DA POLÍCIA")]
        GUA,
        [Display(Name = "2º CABO DA POLÍCIA")]
        SC,
        [Display(Name = "1º CABO DA POLÍCIA")]
        PC,
        [Display(Name = "SARGENTO DA POLÍCIA")]
        SAR,
        [Display(Name = "SARGENTO PRINCIPAL DA POLÍCIA")]
        SAP,
        [Display(Name = "SUBINSPECTOR DA POLÍCIA")]
        SUB,
        [Display(Name = "INSPECTOR DA POLÍCIA")]
        INS,
        [Display(Name = "INSPECTOR PRINCIPAL DA POLÍCIA")]
        INP,
        [Display(Name = "ADJUNTO SUPERINTENDENTE DA POLÍCIA")]
        ASP,
        [Display(Name = "SUPERINTENDENTE DA POLÍCIA")]
        SUP,
        [Display(Name = "SUPERINTENDENTE PRINCIPAL DA POLÍCIA")]
        SPP,
        [Display(Name = "INSPECTOR-GERAL DA POLÍCIA")]
        IPG,
        [Display(Name = "COMISSÁRIO DA POLÍCIA")]
        COM,
        [Display(Name = "1º AJUNTO DE COMISSÁRIO DA POLÍCIA")]
        PAC,
        [Display(Name = "ADJUNTO DE COMISSÁRIO DA POLÍCIA")]
        AJC
    }
    public enum EstadoFuncionario
    {
        ACTIVO,
        TRANSFERIDO,
        EXPULSO,
        FERRIAS,
        FORMAÇÃO,
        AQUARTELADO,
        INACTIVO,
        OBIPTO
    }
    public enum FUNCAO    {
        SENTINELA,
        ADC,
        MOTORISTA,
        [Display(Name = "CHEFE DE SEÇÃO DE LOGÍSTICA E FINANÇAS")]
        ChefeDeLogistica,
        [Display(Name = "CHEFE DE SEÇÃO DE SEGURANÇA PESSOAL")]
        ChefeDeSP,
        [Display(Name = "CHEFE DE SEÇÃO DAS OPERAÇÕES")]
        ChefeDeOP,
        [Display(Name = "CHEFE DE SEÇÃO DE INFORMAÇÃO OPERATIVA")]
        ChefeDeIO,
        [Display(Name = "CHEFE DE SEÇÃO DE INFORMAÇÃO INTERNA")]
        ChefeDeII,
        [Display(Name = "CHEFE DE SEÇÃO DE PROTECÇÃO DE OBJECTO")]
        ChefeDePO,
        [Display(Name = "CHEFE DE SEÇÃO DE GESTÃO DE PESSOAL E FORMAÇÃO")]
        ChefeDeGPF,
        [Display(Name = "CHEFE DE SEÇÃO DA SECRETARIA")]
        ChefeDeSECRE,
        [Display(Name = "CHEFE DE SEÇÃO DE PEAC")]
        ChefeDePEAC,
        [Display(Name = "CHEFE DE SEÇÃO DE DOUTRINA E ÉTICA POLICIAL")]
        ChefeDeDEP,
        [Display(Name = "CHEFE DO SECTOR DE INTENDÊNCIA")]
        ChefeDeINT,
        [Display(Name = "CHEFE DO SECTOR DE ÉTICA E DISCIPLINA")]
        ChefeDeED,
        [Display(Name = "CHEFE DO SECTOR DAS FINANÇAS")]
        ChefeDeFI,
        [Display(Name = "CHEFE DO SECTOR DE SAÚDE")]
        ChefeDeSAU,
        [Display(Name = "COMANDANTE DA SUPAI_MP")]
        ChefeDeCMD,
        [Display(Name = "COMANDANTE DA 1ª COMPANHIA")]
        ChefeDeCMD1C,
        [Display(Name = "COMANDANTE DA 2ª COMPANHIA")]
        ChefeDeCMD2C,
        [Display(Name = "COMANDANTE DA 3ª COMPANHIA")]
        ChefeDeCMD3C,
        [Display(Name = "COMANDANTE DO 1º PELOTÃO")]
        ChefeDeCMD1P,        
        [Display(Name = "COMANDANTE DO 2º PELOTÃO")]
        ChefeDeCMD2P,        
        [Display(Name = "COMANDANTE DO 3º PELOTÃO")]
        ChefeDeCMD3P,        
        [Display(Name = "CHEFE DE SECÇÃO")]
        ChefeDeSCC    
        
    }
    public enum Genero
    {
        F,
        M       
    }
    public class Funcionario
    {
        public int Id { get; set; }

        public int? SeccaoId { get; set; }

        [JsonIgnore]
        public Seccao Seccao { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public string NomeCompleto { get; set; } = string.Empty;

        
        [StringLength(10)]
        public string Nip { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Bi { get; set; } = string.Empty;

        [StringLength(12)]
        public string? Nuit { get; set; } = string.Empty;

        [StringLength(9)]
        public string? Contacto { get; set; } = string.Empty;

        [StringLength(9)]
        public string? C_Alternativo { get; set; } = string.Empty;

        [StringLength(9)]
        public string? C_Familiar { get; set; } = string.Empty;

        [StringLength(1)]
        public GrauParentesco grauParentesco { get; set; } = GrauParentesco.MÃE;

        [StringLength(1)]
        public Genero G { get; set; } = Genero.M;

        [StringLength(50)]
        public EstadoCivil estado_civil { get; set; } = EstadoCivil.Solteiro;

        public DateTime DataNascimento { get; set; }

        public DateTime DataIngresso { get; set; }

        
        public NivelAcademico nivelAcademico { get; set; }= NivelAcademico.Médio;

        [StringLength(100)]
        public Categoria Categoria { get; set; } = Categoria.GUA;

        [StringLength(100)]
        public string Funcao { get; set; } = string.Empty;

        [StringLength(100)]
        public string? LocalTrabalho { get; set; }

        [StringLength(20)]
        public EstadoFuncionario Estado { get; set; } = EstadoFuncionario.ACTIVO;

        [StringLength(100)]
        public string? Bairro { get; set; } = string.Empty;
        [StringLength(3)]
        public string? Quarterao_N { get; set; } = string.Empty;
        [StringLength(5)]
        public string? Casa_N { get; set; } = string.Empty;

        public string? FotografiaUrl { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        public ICollection<Ferias> Ferias { get; set; } = new List<Ferias>();

       
    }
}
