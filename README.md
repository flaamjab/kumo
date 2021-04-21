# Kumo

Enriching Microsoft Word documents with semantic annotations.

![Kumo Icon](kumo.png)

## What is Kumo?

Kumo is a semantic annotation library that enables you to easily specify
relationships between text fragments within a document and places, concepts,
people, dates and other things that may be stored in an ontology.
This is accomplished by providing a simple way to create a relationship
and apply it to one or more text fragments within the document.

## Getting started

> Please note that the library is work in progress.
>
> Some features may be unimplemented, while others may contain major bugs.

To get the latest version of the library, clone this repository
and reference the project under [src/Kumo](src/Kumo) in your solution.

## Examples

The examples here demonstrate some of the basic functionality.

The following code snippet opens a document and retrieves a text fragment.

```C#
using System;
using Kumo;

using (var d = Document.Open("path/to/document.docx"))
{
    var text = d.Range(0, 42).Text();

    Console.WriteLine(text);
}
```

To annotate a text fragment, one must create a Property object
which represents a edge with node connected to it in a semantic net.

```C#
using System;
using Kumo;

using (var d = Document.Open("path/to/document.docx"))
{
    var range = d.Range(0, 42);
    
    // The strings used to create a Property must be valid URIs.
    var property = new Property(
        "http://example.org/predicate",
        "http://example.org/value"
    );

    // Annotate the text fragment with this property.
    range.Attach(property);
}
```

Accessing all annotated text fragments within a document
is simple.

```C#
using System;
using Kumo;

using (var d = Document.Open("path/to/document.docx"))
{
    // Yes, they are called stars.
    var stars = d.Stars();

    foreach (var s in stars)
    {
        Console.WriteLine(s.Text());
    }
}
```
