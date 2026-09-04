using Microsoft.EntityFrameworkCore;
using supai_mp.Data;
using supai_mp.Models.Organizacao;

namespace supai_mp.Data
{
    public static class OrganizacaoSeed
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            // ============================================================
            // 1. SECÇÕES
            // ============================================================

            var seccoes = new[]
            {
                new
                {
                    Nome = "Gabinete do Comandante",
                    Descricao = "Estrutura de apoio administrativo directo ao Comandante."
                },
                new
                {
                    Nome = "Operações",
                    Descricao = "Coordenação, controlo e acompanhamento das actividades operacionais."
                },
                new
                {
                    Nome = "Doutrina e Ética Policial",
                    Descricao = "Responsável pela doutrina, ética, disciplina comportamental, aprumo e actividades físicas."
                },
                new
                {
                    Nome = "Gestão de Pessoal e Formação",
                    Descricao = "Gestão dos recursos humanos, disciplina e formação."
                },
                new
                {
                    Nome = "PEAC",
                    Descricao = "Planeamento, Estatística, Análise e Controlo."
                },
                new
                {
                    Nome = "Segurança Pessoal",
                    Descricao = "Responsável pela segurança pessoal e escolta de entidades protegidas."
                },
                new
                {
                    Nome = "Protecção de Objectos",
                    Descricao = "Responsável pela protecção e segurança de objectos e instalações."
                },
                new
                {
                    Nome = "Informação Operativa",
                    Descricao = "Recolha, registo, avaliação inicial e canalização de informação operativa."
                },
                new
                {
                    Nome = "Informação Interna",
                    Descricao = "Recolha, registo, avaliação inicial e canalização de informação interna."
                },
                new
                {
                    Nome = "Logística e Finanças",
                    Descricao = "Gestão de saúde, finanças, intendência, armamento e víveres."
                }
            };

            foreach (var item in seccoes)
            {
                if (!await db.Seccoes.AnyAsync(x => x.Nome == item.Nome))
                {
                    db.Seccoes.Add(new Seccao
                    {
                        Nome = item.Nome,
                        Descricao = item.Descricao,
                        Ativo = true,
                        DataCadastro = DateTime.Now
                    });
                }
            }

            await db.SaveChangesAsync();

            // ============================================================
            // 2. SECTORES
            // ============================================================

            var sectores = new[]
            {
                new
                {
                    Nome = "Permanência",
                    Seccao = "Operações",
                    Descricao = "Registo diário de todas as actividades da subunidade."
                },
                new
                {
                    Nome = "Planificação",
                    Seccao = "Operações",
                    Descricao = "Planeamento das actividades e elaboração das escalas de serviço."
                },
                new
                {
                    Nome = "Doutrina e Ética Policial",
                    Seccao = "Doutrina e Ética Policial",
                    Descricao = "Palestras, doutrina, ética, aprumo, disciplina e postura profissional."
                },
                new
                {
                    Nome = "Desporto",
                    Seccao = "Doutrina e Ética Policial",
                    Descricao = "Actividades desportivas e físicas."
                },
                new
                {
                    Nome = "Gestão de Pessoal",
                    Seccao = "Gestão de Pessoal e Formação",
                    Descricao = "Gestão administrativa dos funcionários."
                },
                new
                {
                    Nome = "Disciplina",
                    Seccao = "Gestão de Pessoal e Formação",
                    Descricao = "Tratamento e acompanhamento dos processos disciplinares."
                },
                new
                {
                    Nome = "Formação",
                    Seccao = "Gestão de Pessoal e Formação",
                    Descricao = "Área destinada à formação dos funcionários."
                },
                new
                {
                    Nome = "Saúde",
                    Seccao = "Logística e Finanças",
                    Descricao = "Registo e acompanhamento dos funcionários doentes."
                },
                new
                {
                    Nome = "Finanças",
                    Seccao = "Logística e Finanças",
                    Descricao = "Apoio na resolução de problemas relacionados com salários."
                },
                new
                {
                    Nome = "Intendência",
                    Seccao = "Logística e Finanças",
                    Descricao = "Distribuição de uniformes."
                },
                new
                {
                    Nome = "Armamento",
                    Seccao = "Logística e Finanças",
                    Descricao = "Levantamento e devolução de armas utilizadas no serviço."
                },
                new
                {
                    Nome = "Víveres",
                    Seccao = "Logística e Finanças",
                    Descricao = "Garantia da alimentação da força."
                },
                new
                {
                    Nome = "T.O.",
                    Seccao = "Segurança Pessoal",
                    Descricao = "Coordenação administrativa dos T.O. afectos às equipas e aos postos de segurança."
                }
            };

            foreach (var item in sectores)
            {
                var seccao = await db.Seccoes
                    .FirstOrDefaultAsync(x => x.Nome == item.Seccao);

                if (seccao == null)
                    continue;

                if (!await db.Sectores.AnyAsync(x =>
                    x.Nome == item.Nome &&
                    x.SeccaoId == seccao.Id))
                {
                    db.Sectores.Add(new Sector
                    {
                        Nome = item.Nome,
                        SeccaoId = seccao.Id,
                        Descricao = item.Descricao,
                        Ativo = true,
                        DataCadastro = DateTime.Now
                    });
                }
            }

            await db.SaveChangesAsync();

            // ============================================================
            // 3. FUNÇÕES OPERACIONAIS
            // ============================================================

            var funcoes = new[]
            {
                new
                {
                    Nome = "Comandante",
                    Descricao = "Autoridade máxima da subunidade."
                },
                new
                {
                    Nome = "Chefe da Secção",
                    Descricao = "Responsável máximo pela respectiva secção."
                },
                new
                {
                    Nome = "Chefe de Secretaria",
                    Descricao = "Responsável pela Secretaria do Gabinete do Comandante."
                },
                new
                {
                    Nome = "Funcionário Administrativo",
                    Descricao = "Funcionário afecto às actividades administrativas."
                },
                new
                {
                    Nome = "Chefe de Operações",
                    Descricao = "Responsável pela coordenação e validação das actividades operacionais."
                },
                new
                {
                    Nome = "Oficial de Permanência",
                    Descricao = "Responsável pelo serviço de permanência durante 24 horas."
                },
                new
                {
                    Nome = "Chefe de Planificação",
                    Descricao = "Responsável pelo Sector de Planificação."
                },
                new
                {
                    Nome = "Funcionário da Planificação",
                    Descricao = "Funcionário afecto à planificação e elaboração de escalas."
                },
                new
                {
                    Nome = "Chefe da Ordem",
                    Descricao = "Responsável pela ordem e disciplina da subunidade."
                },
                new
                {
                    Nome = "Chefe da Secção de Doutrina",
                    Descricao = "Responsável pela Secção de Doutrina e Ética Policial."
                },
                new
                {
                    Nome = "Funcionário de Doutrina e Ética",
                    Descricao = "Funcionário afecto à doutrina, ética e disciplina comportamental."
                },
                new
                {
                    Nome = "Funcionário de Desporto",
                    Descricao = "Funcionário responsável pelas actividades desportivas e físicas."
                },
                new
                {
                    Nome = "Gestor de Pessoal",
                    Descricao = "Responsável pela gestão administrativa dos funcionários."
                },
                new
                {
                    Nome = "Técnico de Pessoal",
                    Descricao = "Técnico que apoia directamente o Gestor de Pessoal."
                },
                new
                {
                    Nome = "Responsável da Disciplina",
                    Descricao = "Responsável pelos processos disciplinares."
                },
                new
                {
                    Nome = "Funcionário de Formação",
                    Descricao = "Funcionário afecto à área de formação."
                },
                new
                {
                    Nome = "Técnico PEAC",
                    Descricao = "Técnico afecto ao Planeamento, Estatística, Análise e Controlo."
                },
                new
                {
                    Nome = "Chefe da Escolta",
                    Descricao = "Responsável pela coordenação de uma escolta."
                },
                new
                {
                    Nome = "Chefe da Equipa",
                    Descricao = "Responsável pela coordenação de uma equipa."
                },
                new
                {
                    Nome = "ADC",
                    Descricao = "Afecto à actividade de segurança pessoal."
                },
                new
                {
                    Nome = "Motorista",
                    Descricao = "Responsável pela condução dos meios afectos à segurança."
                },
                new
                {
                    Nome = "Atirador",
                    Descricao = "Funcionário com função operacional de atirador."
                },
                new
                {
                    Nome = "Membro de Segurança Pessoal",
                    Descricao = "Funcionário afecto à segurança pessoal."
                },
                new
                {
                    Nome = "T.O.",
                    Descricao = "Funcionário afecto ao serviço de T.O."
                },
                new
                {
                    Nome = "Comandante de Companhia",
                    Descricao = "Responsável por uma companhia da Protecção de Objectos."
                },
                new
                {
                    Nome = "Comandante de Pelotão",
                    Descricao = "Responsável por um pelotão da Protecção de Objectos."
                },
                new
                {
                    Nome = "Guarda",
                    Descricao = "Funcionário afecto à protecção de objectos e instalações."
                },
                new
                {
                    Nome = "Operativo",
                    Descricao = "Funcionário afecto à recolha e canalização de informação."
                },
                new
                {
                    Nome = "Responsável de Saúde",
                    Descricao = "Responsável pelo registo de informações dos funcionários doentes."
                },
                new
                {
                    Nome = "Responsável de Finanças",
                    Descricao = "Responsável pelo apoio na resolução de problemas salariais."
                },
                new
                {
                    Nome = "Funcionário de Intendência",
                    Descricao = "Funcionário responsável pela distribuição de uniformes."
                },
                new
                {
                    Nome = "Funcionário de Armamento",
                    Descricao = "Funcionário responsável pelo levantamento e devolução de armamento."
                },
                new
                {
                    Nome = "Funcionário de Víveres",
                    Descricao = "Funcionário responsável pela alimentação da força."
                }
            };

            foreach (var item in funcoes)
            {
                if (!await db.FuncoesOperacionais.AnyAsync(x => x.Nome == item.Nome))
                {
                    db.FuncoesOperacionais.Add(new FuncaoOperacional
                    {
                        Nome = item.Nome,
                        Descricao = item.Descricao,
                        Ativo = true
                    });
                }
            }

            await db.SaveChangesAsync();

            // ============================================================
            // 4. TIPOS DE TURNO
            // ============================================================

            var turnos = new[]
            {
                new
                {
                    Nome = "24/48",
                    Descricao = "24 horas de serviço e 48 horas de descanso.",
                    HorasTrabalho = 24,
                    HorasDescanso = 48
                },
                new
                {
                    Nome = "12/24",
                    Descricao = "12 horas de serviço e 24 horas de descanso.",
                    HorasTrabalho = 12,
                    HorasDescanso = 24
                },
                new
                {
                    Nome = "12/36",
                    Descricao = "12 horas de serviço e 36 horas de descanso.",
                    HorasTrabalho = 12,
                    HorasDescanso = 36
                },
                new
                {
                    Nome = "Expediente",
                    Descricao = "Regime normal de expediente.",
                    HorasTrabalho = 8,
                    HorasDescanso = 16
                }
            };

            foreach (var item in turnos)
            {
                if (!await db.TiposTurno.AnyAsync(x => x.Nome == item.Nome))
                {
                    db.TiposTurno.Add(new TipoTurno
                    {
                        Nome = item.Nome,
                        Descricao = item.Descricao,
                        HorasTrabalho = item.HorasTrabalho,
                        HorasDescanso = item.HorasDescanso,
                        Ativo = true
                    });
                }
            }

            await db.SaveChangesAsync();

            // ============================================================
            // 5. UNIDADES OPERACIONAIS - SEGURANÇA PESSOAL
            // ============================================================

            var seccaoSeguranca = await db.Seccoes
                .FirstOrDefaultAsync(x => x.Nome == "Segurança Pessoal");

            if (seccaoSeguranca != null)
            {
                // --------------------------------------------------------
                // ESCOLTA A
                // --------------------------------------------------------

                var escoltaA = await db.UnidadesOperacionais
                    .FirstOrDefaultAsync(x =>
                        x.Nome == "Escolta A" &&
                        x.SeccaoId == seccaoSeguranca.Id);

                if (escoltaA == null)
                {
                    escoltaA = new UnidadeOperacional
                    {
                        Nome = "Escolta A",
                        Tipo = UnidadeOperacional.TipoUnidadeOperacional.Escolta,
                        SeccaoId = seccaoSeguranca.Id,
                        Descricao = "Escolta responsável pela segurança de Sua Excia. Governador da Província de Maputo.",
                        Ativo = true,
                        DataCadastro = DateTime.Now
                    };

                    db.UnidadesOperacionais.Add(escoltaA);
                    await db.SaveChangesAsync();
                }

                // --------------------------------------------------------
                // ESCOLTA B
                // --------------------------------------------------------

                var escoltaB = await db.UnidadesOperacionais
                    .FirstOrDefaultAsync(x =>
                        x.Nome == "Escolta B" &&
                        x.SeccaoId == seccaoSeguranca.Id);

                if (escoltaB == null)
                {
                    escoltaB = new UnidadeOperacional
                    {
                        Nome = "Escolta B",
                        Tipo = UnidadeOperacional.TipoUnidadeOperacional.Escolta,
                        SeccaoId = seccaoSeguranca.Id,
                        Descricao = "Escolta organizada por postos de segurança.",
                        Ativo = true,
                        DataCadastro = DateTime.Now
                    };

                    db.UnidadesOperacionais.Add(escoltaB);
                    await db.SaveChangesAsync();
                }

                // --------------------------------------------------------
                // ESCOLTA C
                // --------------------------------------------------------

                var escoltaC = await db.UnidadesOperacionais
                    .FirstOrDefaultAsync(x =>
                        x.Nome == "Escolta C" &&
                        x.SeccaoId == seccaoSeguranca.Id);

                if (escoltaC == null)
                {
                    escoltaC = new UnidadeOperacional
                    {
                        Nome = "Escolta C",
                        Tipo = UnidadeOperacional.TipoUnidadeOperacional.Escolta,
                        SeccaoId = seccaoSeguranca.Id,
                        Descricao = "Escolta responsável pela segurança de Sua Excia. Secretário de Estado da Província de Maputo.",
                        Ativo = true,
                        DataCadastro = DateTime.Now
                    };

                    db.UnidadesOperacionais.Add(escoltaC);
                    await db.SaveChangesAsync();
                }

                // ========================================================
                // 6. EQUIPAS DA ESCOLTA A
                // ========================================================

                for (int i = 1; i <= 3; i++)
                {
                    string nomeEquipa = $"Equipa {i}";

                    if (!await db.Equipas.AnyAsync(x =>
                        x.Nome == nomeEquipa &&
                        x.UnidadeOperacionalId == escoltaA.Id))
                    {
                        db.Equipas.Add(new Equipa
                        {
                            Nome = nomeEquipa,
                            UnidadeOperacionalId = escoltaA.Id,
                            Descricao = $"Equipa {i} da Escolta A.",
                            Ativo = true,
                            DataCadastro = DateTime.Now
                        });
                    }
                }

                // ========================================================
                // 7. EQUIPAS DA ESCOLTA C
                // ========================================================

                for (int i = 1; i <= 3; i++)
                {
                    string nomeEquipa = $"Equipa {i}";

                    if (!await db.Equipas.AnyAsync(x =>
                        x.Nome == nomeEquipa &&
                        x.UnidadeOperacionalId == escoltaC.Id))
                    {
                        db.Equipas.Add(new Equipa
                        {
                            Nome = nomeEquipa,
                            UnidadeOperacionalId = escoltaC.Id,
                            Descricao = $"Equipa {i} da Escolta C.",
                            Ativo = true,
                            DataCadastro = DateTime.Now
                        });
                    }
                }

                await db.SaveChangesAsync();

                // ========================================================
                // 8. POSTOS DA ESCOLTA B
                // ========================================================

                var postosB = new[]
                {
                    new
                    {
                        Codigo = "SP-B-001",
                        Nome = "Posto de Matutuíne",
                        Localizacao = "Matutuíne"
                    },
                    new
                    {
                        Codigo = "SP-B-002",
                        Nome = "Posto de Namaacha",
                        Localizacao = "Namaacha"
                    },
                    new
                    {
                        Codigo = "SP-B-003",
                        Nome = "Posto da Manhiça",
                        Localizacao = "Manhiça"
                    },
                    new
                    {
                        Codigo = "SP-B-004",
                        Nome = "Posto de Magude",
                        Localizacao = "Magude"
                    },
                    new
                    {
                        Codigo = "SP-B-005",
                        Nome = "Posto de Marracuene",
                        Localizacao = "Marracuene"
                    }
                };

                foreach (var item in postosB)
                {
                    if (!await db.Postos.AnyAsync(x => x.Codigo == item.Codigo))
                    {
                        db.Postos.Add(new Posto
                        {
                            Codigo = item.Codigo,
                            Nome = item.Nome,
                            Localizacao = item.Localizacao,
                            SeccaoId = seccaoSeguranca.Id,
                            UnidadeOperacionalId = escoltaB.Id,
                            Descricao = "Posto da Escolta B. Mínimo de 2 ADCs.",
                            Ativo = true,
                            DataCadastro = DateTime.Now
                        });
                    }
                }

                await db.SaveChangesAsync();

                // ========================================================
                // 9. POSTOS DE T.O.
                // ========================================================

                var sectorTO = await db.Sectores
                    .FirstOrDefaultAsync(x =>
                        x.Nome == "T.O." &&
                        x.SeccaoId == seccaoSeguranca.Id);

                var postosTO = new[]
                {
                    new
                    {
                        Codigo = "TO-001",
                        Nome = "Residência do Governador",
                        Localizacao = "Residência do Governador"
                    },
                    new
                    {
                        Codigo = "TO-002",
                        Nome = "Residência do Secretário de Estado",
                        Localizacao = "Residência do Secretário de Estado"
                    },
                    new
                    {
                        Codigo = "TO-003",
                        Nome = "Gabinete do Governador",
                        Localizacao = "Gabinete do Governador"
                    },
                    new
                    {
                        Codigo = "TO-004",
                        Nome = "Gabinete do Secretário de Estado",
                        Localizacao = "Gabinete do Secretário de Estado"
                    }
                };

                foreach (var item in postosTO)
                {
                    if (!await db.Postos.AnyAsync(x => x.Codigo == item.Codigo))
                    {
                        db.Postos.Add(new Posto
                        {
                            Codigo = item.Codigo,
                            Nome = item.Nome,
                            Localizacao = item.Localizacao,
                            SeccaoId = seccaoSeguranca.Id,
                            SectorId = sectorTO?.Id,
                            Descricao = "Posto destinado à afectação de T.O.",
                            Ativo = true,
                            DataCadastro = DateTime.Now
                        });
                    }
                }

                await db.SaveChangesAsync();
            }

            // ============================================================
            // 10. PROTECÇÃO DE OBJECTOS
            // ============================================================

            var seccaoObjectos = await db.Seccoes
                .FirstOrDefaultAsync(x => x.Nome == "Protecção de Objectos");

            if (seccaoObjectos != null)
            {
                // --------------------------------------------------------
                // 1.ª COMPANHIA
                // --------------------------------------------------------

                var companhia1 = await db.UnidadesOperacionais
                    .FirstOrDefaultAsync(x =>
                        x.Nome == "1.ª Companhia" &&
                        x.SeccaoId == seccaoObjectos.Id);

                if (companhia1 == null)
                {
                    companhia1 = new UnidadeOperacional
                    {
                        Nome = "1.ª Companhia",
                        Tipo = UnidadeOperacional.TipoUnidadeOperacional.Companhia,
                        SeccaoId = seccaoObjectos.Id,
                        Descricao = "1.ª Companhia de Protecção de Objectos.",
                        Ativo = true,
                        DataCadastro = DateTime.Now
                    };

                    db.UnidadesOperacionais.Add(companhia1);
                    await db.SaveChangesAsync();
                }

                // --------------------------------------------------------
                // 2.ª COMPANHIA
                // --------------------------------------------------------

                var companhia2 = await db.UnidadesOperacionais
                    .FirstOrDefaultAsync(x =>
                        x.Nome == "2.ª Companhia" &&
                        x.SeccaoId == seccaoObjectos.Id);

                if (companhia2 == null)
                {
                    companhia2 = new UnidadeOperacional
                    {
                        Nome = "2.ª Companhia",
                        Tipo = UnidadeOperacional.TipoUnidadeOperacional.Companhia,
                        SeccaoId = seccaoObjectos.Id,
                        Descricao = "2.ª Companhia de Protecção de Objectos.",
                        Ativo = true,
                        DataCadastro = DateTime.Now
                    };

                    db.UnidadesOperacionais.Add(companhia2);
                    await db.SaveChangesAsync();
                }

                // --------------------------------------------------------
                // 3.ª COMPANHIA
                // --------------------------------------------------------

                var companhia3 = await db.UnidadesOperacionais
                    .FirstOrDefaultAsync(x =>
                        x.Nome == "3.ª Companhia" &&
                        x.SeccaoId == seccaoObjectos.Id);

                if (companhia3 == null)
                {
                    companhia3 = new UnidadeOperacional
                    {
                        Nome = "3.ª Companhia",
                        Tipo = UnidadeOperacional.TipoUnidadeOperacional.Companhia,
                        SeccaoId = seccaoObjectos.Id,
                        Descricao = "3.ª Companhia de Protecção de Objectos.",
                        Ativo = true,
                        DataCadastro = DateTime.Now
                    };

                    db.UnidadesOperacionais.Add(companhia3);
                    await db.SaveChangesAsync();
                }

                // ========================================================
                // 11. PELOTÕES
                // ========================================================

                var companhias = new[]
                {
                    companhia1,
                    companhia2,
                    companhia3
                };

                foreach (var companhia in companhias)
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        string nomePelotao = $"{i}.º Pelotão";

                        if (!await db.UnidadesOperacionais.AnyAsync(x =>
                            x.Nome == nomePelotao &&
                            x.SeccaoId == seccaoObjectos.Id &&
                            x.UnidadePaiId == companhia.Id))
                        {
                            db.UnidadesOperacionais.Add(new UnidadeOperacional
                            {
                                Nome = nomePelotao,
                                Tipo = UnidadeOperacional.TipoUnidadeOperacional.Pelotao,
                                SeccaoId = seccaoObjectos.Id,
                                UnidadePaiId = companhia.Id,
                                Descricao = $"{nomePelotao} da {companhia.Nome}.",
                                Ativo = true,
                                DataCadastro = DateTime.Now
                            });
                        }
                    }

                    await db.SaveChangesAsync();
                }

                // ========================================================
                // 12. POSTOS DA PROTECÇÃO DE OBJECTOS
                // DISTRIBUIÇÃO PROVISÓRIA
                // ========================================================

                var distribuicaoPostos = new[]
                {
                    new
                    {
                        Codigo = "PO-001",
                        Nome = "Residência do Antigo Comandante-Geral",
                        Companhia = "1.ª Companhia",
                        Pelotao = "1.º Pelotão"
                    },
                    new
                    {
                        Codigo = "PO-002",
                        Nome = "Residência de Sua Excia. Governador",
                        Companhia = "1.ª Companhia",
                        Pelotao = "2.º Pelotão"
                    },
                    new
                    {
                        Codigo = "PO-003",
                        Nome = "Residência de Sua Excia. Secretário de Estado",
                        Companhia = "2.ª Companhia",
                        Pelotao = "1.º Pelotão"
                    },
                    new
                    {
                        Codigo = "PO-004",
                        Nome = "Gabinete de Sua Excia. Governador da Província de Maputo",
                        Companhia = "2.ª Companhia",
                        Pelotao = "2.º Pelotão"
                    },
                    new
                    {
                        Codigo = "PO-005",
                        Nome = "Gabinete de Sua Excia. Secretário de Estado da Província de Maputo",
                        Companhia = "3.ª Companhia",
                        Pelotao = "1.º Pelotão"
                    },
                    new
                    {
                        Codigo = "PO-006",
                        Nome = "Assembleia da República da Província de Maputo",
                        Companhia = "3.ª Companhia",
                        Pelotao = "2.º Pelotão"
                    }
                };

                foreach (var item in distribuicaoPostos)
                {
                    var companhia = companhias
                        .FirstOrDefault(x => x.Nome == item.Companhia);

                    if (companhia == null)
                        continue;

                    var pelotao = await db.UnidadesOperacionais
                        .FirstOrDefaultAsync(x =>
                            x.Nome == item.Pelotao &&
                            x.UnidadePaiId == companhia.Id);

                    if (pelotao == null)
                        continue;

                    if (!await db.Postos.AnyAsync(x => x.Codigo == item.Codigo))
                    {
                        db.Postos.Add(new Posto
                        {
                            Codigo = item.Codigo,
                            Nome = item.Nome,
                            SeccaoId = seccaoObjectos.Id,
                            UnidadeOperacionalId = pelotao.Id,
                            Descricao = "Posto da Protecção de Objectos. Distribuição provisória. Mínimo de 3 funcionários.",
                            Ativo = true,
                            DataCadastro = DateTime.Now
                        });
                    }
                }

                await db.SaveChangesAsync();
            }

            // ============================================================
            // 13. INFORMAÇÃO OPERATIVA
            // ============================================================

            // Não existem sectores, equipas ou postos.
            // Os funcionários ficam directamente afectos à secção
            // com a função de Operativo.

            // ============================================================
            // 14. INFORMAÇÃO INTERNA
            // ============================================================

            // Não existem sectores, equipas ou postos.
            // Os funcionários ficam directamente afectos à secção
            // com a função de Operativo.

            // ============================================================
            // FIM DO SEED
            // ============================================================

            await db.SaveChangesAsync();
        }
    }
}