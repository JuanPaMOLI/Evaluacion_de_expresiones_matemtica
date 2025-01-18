using System;
using System.Collections.Generic;
using System.Globalization;

//Determina los nodos hijos y nodo
class ExpressionNode
{
    public string Value { get; set; }
    public ExpressionNode? Left { get; set; }
    public ExpressionNode? Right { get; set; }

    public ExpressionNode(string value)
    {
        Value = value;
    }
}

class ExpressionTree
{
    //Estos primeros funciona para realizar los cambios entre la calculadora logica y artimetica
    private readonly bool isLogicalMode;


    public ExpressionNode? Root { get; private set; }

    public ExpressionTree(bool isLogicalMode = false)
    {
        this.isLogicalMode = isLogicalMode;
    }

    //Funcion que se encarga de crear el arbol
    public void BuildTree(string postfixExpression)
    {
        Stack<ExpressionNode> stack = new Stack<ExpressionNode>();
        string[] tokens = PreprocessExpression(postfixExpression);

        foreach (string token in tokens)
        {
            if (IsOperator(token))
            {
                var node = new ExpressionNode(token);
                // Este codigo es importante para la formacion del arbol, ya que al pasar por cada token genera un nuevo nodo para los hijos.
                // Para operadores unarios (~)
                if (token == "~")
                {
                    if (stack.Count < 1)
                        throw new InvalidOperationException("Expresión inválida: faltan operandos para el operador NOT (~).");

                    node.Left = stack.Pop();
                }
                // Para operadores binarios
                else
                {
                    if (stack.Count < 2)
                        throw new InvalidOperationException($"Expresión inválida: faltan operandos para el operador {token}.");

                    node.Right = stack.Pop();
                    node.Left = stack.Pop();
                }
                stack.Push(node);
            }
            else
            {
                stack.Push(new ExpressionNode(token));
            }
        }

        if (stack.Count != 1)
        {
            throw new InvalidOperationException("Expresión inválida: faltan operadores.");
        }

        Root = stack.Pop();
    }

    public double Evaluate()
    {
        if (Root == null)
            throw new InvalidOperationException("El árbol está vacío.");

        return EvaluateNode(Root);
    }

    private double EvaluateNode(ExpressionNode node)
    {
        //Es para garantizar que en el modo logico solo acepte 1 y 0
        if (!IsOperator(node.Value))
        {
            double value = ParseNumber(node.Value);
            if (isLogicalMode && (value != 0 && value != 1))
                throw new InvalidOperationException("Solo se permiten valores binarios (0 y 1) para operaciones lógicas.");
            
            return value;
        }

        double left = node.Left != null ? EvaluateNode(node.Left) : 0;
        double right = node.Right != null ? EvaluateNode(node.Right) : 0;

        if (isLogicalMode)
        {
            return node.Value switch
                //Determinar las operaciones logicas de la calculadora
            {
                "|" => (left != 0 || right != 0) ? 1 : 0,   // OR
                "&" => (left != 0 && right != 0) ? 1 : 0,  // AND
                "^" => (left != right) ? 1 : 0,            // XOR
                "~" => (left == 0) ? 1 : 0,                // NOT
                _ => throw new InvalidOperationException($"Operador desconocido: {node.Value}"),
            };
        }
        else
        {
            return node.Value switch
            {
                "+" => left + right,
                "-" => left - right,
                "*" => left * right,
                "/" => left / right,
                "%" => left * (right / 100),
                "**" => Math.Pow(left, right),
                _ => throw new InvalidOperationException($"Operador desconocido: {node.Value}"),
            };
        }
    }

    private bool IsOperator(string value)
    {
        if (isLogicalMode)
        {
            return value == "|" || value == "&" || value == "^" || value == "~";
        }
        else
        {
            return value == "+" || value == "-" || value == "*" || value == "/" || value == "%" || value == "**";
        }
    }

private double ParseNumber(string token)
{
    //Se encarga del funcionamiento y fusionamiento de los numeros con decimales, ademas de verificar los 1 y 0 en el modo logico
    string adjustedToken = token.Replace(',', '.');
    if (double.TryParse(adjustedToken, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
    {
        if (isLogicalMode && result != 0 && result != 1)
        {
            throw new InvalidOperationException("En modo lógico, solo se permiten valores binarios (0 y 1).");
        }
        return result;
    }

    throw new FormatException($"No se pudo interpretar el número: {token}");
}

    //Funcion que se encarga de convertir las expresiones en token para que sean procesadas por los procesos anteriores Ejemplo: La expresión "3 4 +" se divide en tokens: ["3", "4", "+"]
    private string[] PreprocessExpression(string expression)
    {
        List<string> tokens = new List<string>();
        string[] rawTokens = expression.Split(' ');

        foreach (string token in rawTokens)
        {
            if (string.IsNullOrWhiteSpace(token))
                continue;

            // Intenta analizar el token como un número
            if (double.TryParse(token.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                tokens.Add(token.Replace(',', '.')); // Convierte ',' a '.'
            }
            else if (IsOperator(token))
            {
                tokens.Add(token); // Es un operador válido
            }
            else
            {
                throw new FormatException($"Token inválido encontrado: {token}");
            }
        }

        return tokens.ToArray();
    }
}