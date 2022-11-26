using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EjemploReceta
{

    internal class Receta
    {
        public Receta(int id, string nombre)
            => (this.id, this.nombre) = (id, nombre);

        public int id;
        public string nombre;

        public string ObtenerInsert()
        {
            return ($"INSERT INTO Recetas (id, nombre) " +
                    $"VALUES ({this.id}, {this.nombre});");
        }
    }

    internal class Ingrediente
    {
        public Ingrediente(int id, string nombre, float cantidad) 
            => (this.id, this.nombre, this.cantidad) = (id, nombre, cantidad);

        public int id;
        public int? idReceta;
        public string nombre;
        public float cantidad;

        public string ObtenerInsert()
        {
            return ($"INSERT INTO Ingredientes (id, id_receta, nombre, cantidad) " +
                    $"VALUES ({this.id}, {this.idReceta}, {this.nombre}, {this.cantidad});");
        }
    }

    internal class RecetaToSQL : RecetaBaseVisitor<object>
    {
        List<string> inserts = new List<string>();

        public List<string> Inserts { get => inserts; }

        private int contadorReceta = 0;
        private int contadorIngrediente = 0;

        public override object VisitDet_ingredientes([NotNull] RecetaParser.Det_ingredientesContext context)
        {
            float cantidad = float.Parse(context.NUM().GetText());
            string nombre = context.TEXT().GetText();
            contadorIngrediente++;
            Ingrediente ingrediente = new Ingrediente(contadorIngrediente, nombre, cantidad);

            return ingrediente;
        }

        public override object VisitIngredientes([NotNull] RecetaParser.IngredientesContext context)
        {
            List<Ingrediente> ingredientes = new List<Ingrediente>();
            foreach (var ingrediente_tree in context.det_ingredientes())
            {
                ingredientes.Add((Ingrediente) Visit(ingrediente_tree));
            }
            return ingredientes;
        }

        public override object VisitNombre([NotNull] RecetaParser.NombreContext context)
        {
            return context.TEXT().GetText();
        }

        public override object VisitReceta([NotNull] RecetaParser.RecetaContext context)
        {
            string nombre = ((string) Visit(context.nombre()));
            contadorReceta++;
            Receta receta = new Receta(contadorReceta, nombre);
            List<Ingrediente> ingredientes = (List<Ingrediente>) Visit(context.ingredientes());


            inserts.Add(receta.ObtenerInsert());

            foreach (var ingrediente in ingredientes)
            {
                ingrediente.idReceta = receta.id;
                inserts.Add(ingrediente.ObtenerInsert());
            }

            return new Object();
        }

        public override object VisitProgram([NotNull] RecetaParser.ProgramContext context)
        {
            base.VisitProgram(context);
            string output = "";
            foreach (var insert in inserts)
                output += insert + "\n";

            return output;
        }
    }
}
