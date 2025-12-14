namespace spinner;

internal class MissingKeyAttributeException(string attribute)
    : Exception($"Missing Attribute {attribute}") { }
