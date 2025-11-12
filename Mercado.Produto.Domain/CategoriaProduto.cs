using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercado.Produto.Domain;

// PORQUÊ: Garante a integridade dos tipos (Artefato 5).
// Evita "strings mágicas" no código.
public enum CategoriaProduto
{
    Laticinios,
    Hortifruti,
    Padaria,
    Mercearia,
    Acougue,
    Bebidas,
    Limpeza
}