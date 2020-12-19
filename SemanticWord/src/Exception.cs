using System;

public class FileNotFoundException : Exception
{
    public FileNotFoundException() { }
    public FileNotFoundException(string message) :
        base(message)
    { }
}

public class FileIsInvalidException : Exception
{
    public FileIsInvalidException() { }
    public FileIsInvalidException(string message) :
        base(message)
    { }
}

public class MetatagNotFoundException : Exception
{
    public MetatagNotFoundException() { }

    public MetatagNotFoundException(string message) :
        base(message)
    { }
}

public class InvalidMetatagException : Exception
{
    public InvalidMetatagException() { }

    public InvalidMetatagException(string message) :
        base(message)
    { }
}