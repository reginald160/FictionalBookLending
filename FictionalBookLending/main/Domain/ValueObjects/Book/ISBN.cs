namespace FictionalBookLending.src.Domain.ValueObjects.Book
{
    public  record struct ISBN(string Value)
    {
        
        public bool IsValid => !string.IsNullOrWhiteSpace(Value)
                               && Value.Length == 12
                               && Value.All(char.IsDigit);
        public static ISBN Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("ISBN cannot be empty", nameof(value));

            var normalized = value.Replace("-", "").Trim();

            var isbn = new ISBN(normalized);

            if (!isbn.IsValid)
                throw new ArgumentException("Invalid ISBN: must be 12 digits and numeric only.", nameof(value));

            return isbn;
        }

        public override string ToString() => Value;
    }
}
