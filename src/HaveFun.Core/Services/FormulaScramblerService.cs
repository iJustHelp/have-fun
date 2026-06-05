namespace HaveFun.Core;

public sealed class FormulaScramblerService
{
    private const double NumericTolerance = 0.000000001;
    private const string SupportedOperatorCharacters = "+-*/()=";

    public IReadOnlyList<Tile> CreateFormulaTiles(CurrentRound round)
    {
        return CreateFormulaTiles(round.Text);
    }

    public IReadOnlyList<Tile> CreateFormulaTiles(string formula)
    {
        var normalizedFormula = NormalizeFormula(formula);

        if (!IsMathematicallyCorrect(normalizedFormula))
        {
            throw new ArgumentException("Formula must be a valid equation.", nameof(formula));
        }

        var formulaParts = TokenizeFormula(normalizedFormula)
            .ToArray();

        for (var index = formulaParts.Length - 1; index > 0; index--)
        {
            var swapIndex = Random.Shared.Next(index + 1);
            (formulaParts[index], formulaParts[swapIndex]) = (formulaParts[swapIndex], formulaParts[index]);
        }

        return formulaParts
            .Select(part => new Tile
            {
                Id = Guid.NewGuid(),
                Text = part
            })
            .ToArray();
    }

    public int CalculateScore(CurrentRound round, IReadOnlyList<Tile> selectedTiles)
    {
        return CalculateScore(round.Text, JoinTiles(selectedTiles));
    }

    public int CalculateScore(string sourceFormula, string submittedFormula)
    {
        var normalizedSourceFormula = NormalizeFormula(sourceFormula);
        var normalizedSubmittedFormula = NormalizeFormula(submittedFormula);

        if (normalizedSourceFormula.Length == 0)
        {
            return 0;
        }

        return IsMathematicallyCorrect(normalizedSubmittedFormula)
            ? CountNumbers(normalizedSubmittedFormula)
            : 0;
    }

    public int GetTotalScore(string sourceFormula)
    {
        return CountNumbers(NormalizeFormula(sourceFormula));
    }

    public string JoinTiles(IReadOnlyList<Tile> tiles)
    {
        return string.Concat(tiles.Select(tile => tile.Text));
    }

    public int CountNumbers(string formula)
    {
        return TokenizeFormula(formula).Count(IsNumberToken);
    }

    public IReadOnlyList<string> TokenizeFormula(string formula)
    {
        var normalizedFormula = NormalizeFormula(formula);
        var parts = new List<string>();

        for (var index = 0; index < normalizedFormula.Length;)
        {
            var character = normalizedFormula[index];

            if (character == '=')
            {
                index++;

                if (index < normalizedFormula.Length && char.IsDigit(normalizedFormula[index]))
                {
                    var numberStartIndex = index;

                    while (index < normalizedFormula.Length && char.IsDigit(normalizedFormula[index]))
                    {
                        index++;
                    }

                    parts.Add("=" + normalizedFormula[numberStartIndex..index]);
                    continue;
                }

                if (parts.Count > 0 && IsNumberToken(parts[^1]))
                {
                    parts[^1] += "=";
                    continue;
                }

                parts.Add(character.ToString());
                continue;
            }

            if (!char.IsDigit(character))
            {
                parts.Add(character.ToString());
                index++;
                continue;
            }

            var startIndex = index;

            while (index < normalizedFormula.Length && char.IsDigit(normalizedFormula[index]))
            {
                index++;
            }

            parts.Add(normalizedFormula[startIndex..index]);
        }

        return parts;
    }

    public bool IsMathematicallyCorrect(string formula)
    {
        var normalizedFormula = NormalizeFormula(formula);

        if (!HasEquationAndNumbers(normalizedFormula))
        {
            return false;
        }

        var equationSides = normalizedFormula.Split('=');

        if (equationSides[0].Length == 0 || equationSides[1].Length == 0)
        {
            return false;
        }

        return TryEvaluateExpression(equationSides[0], out var leftValue) &&
            TryEvaluateExpression(equationSides[1], out var rightValue) &&
            Math.Abs(leftValue - rightValue) <= NumericTolerance;
    }

    public string NormalizeFormula(string formula)
    {
        if (string.IsNullOrWhiteSpace(formula))
        {
            return string.Empty;
        }

        return string.Concat(formula.Where(character => !char.IsWhiteSpace(character)));
    }

    private static bool IsSupportedCharacter(char character)
    {
        return char.IsDigit(character) || SupportedOperatorCharacters.Contains(character);
    }

    private static bool IsNumberToken(string part)
    {
        return part.Any(char.IsDigit);
    }

    private static bool HasEquationAndNumbers(string formula)
    {
        if (formula.Length == 0 || formula.Count(character => character == '=') != 1)
        {
            return false;
        }

        var equationSides = formula.Split('=');
        return equationSides[0].Any(char.IsDigit) && equationSides[1].Any(char.IsDigit);
    }

    private static bool TryEvaluateExpression(string expression, out double value)
    {
        value = 0;

        if (expression.Length == 0 || expression.Any(character => !IsSupportedCharacter(character) || character == '='))
        {
            return false;
        }

        var parser = new ExpressionParser(expression);
        return parser.TryParse(out value);
    }

    private sealed class ExpressionParser
    {
        private readonly string _expression;
        private int _index;

        public ExpressionParser(string expression)
        {
            _expression = expression;
        }

        public bool TryParse(out double value)
        {
            value = 0;

            if (!TryParseExpression(out value))
            {
                return false;
            }

            return _index == _expression.Length;
        }

        private bool TryParseExpression(out double value)
        {
            if (!TryParseTerm(out value))
            {
                return false;
            }

            while (TryRead('+') || TryRead('-'))
            {
                var operatorCharacter = _expression[_index - 1];

                if (!TryParseTerm(out var nextValue))
                {
                    return false;
                }

                value = operatorCharacter == '+'
                    ? value + nextValue
                    : value - nextValue;
            }

            return true;
        }

        private bool TryParseTerm(out double value)
        {
            if (!TryParseFactor(out value))
            {
                return false;
            }

            while (TryRead('*') || TryRead('/'))
            {
                var operatorCharacter = _expression[_index - 1];

                if (!TryParseFactor(out var nextValue))
                {
                    return false;
                }

                if (operatorCharacter == '/')
                {
                    if (Math.Abs(nextValue) <= NumericTolerance)
                    {
                        return false;
                    }

                    value /= nextValue;
                }
                else
                {
                    value *= nextValue;
                }
            }

            return true;
        }

        private bool TryParseFactor(out double value)
        {
            value = 0;

            if (TryRead('+'))
            {
                return TryParseFactor(out value);
            }

            if (TryRead('-'))
            {
                if (!TryParseFactor(out value))
                {
                    return false;
                }

                value = -value;
                return true;
            }

            if (TryRead('('))
            {
                if (!TryParseExpression(out value) || !TryRead(')'))
                {
                    return false;
                }

                return true;
            }

            return TryParseNumber(out value);
        }

        private bool TryParseNumber(out double value)
        {
            value = 0;
            var startIndex = _index;

            while (_index < _expression.Length && char.IsDigit(_expression[_index]))
            {
                _index++;
            }

            return _index > startIndex &&
                double.TryParse(
                    _expression.AsSpan(startIndex, _index - startIndex),
                    System.Globalization.NumberStyles.None,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out value);
        }

        private bool TryRead(char character)
        {
            if (_index >= _expression.Length || _expression[_index] != character)
            {
                return false;
            }

            _index++;
            return true;
        }
    }
}
