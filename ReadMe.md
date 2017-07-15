# Newtonsoft.Json Utils
Utility classes for use with [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json):
## PrivateFieldsJsonConverter
### What 
A JsonConverter that serializes and deserializes only private fields of a root object (child objects are serialized normally). Additionally,
when deserializing, creates a collection of fields that are missing from the source json as well as a 
collection of fields that are present in the souce json but are not members of the target type.
### Why
Consider a behaviourless type used for configuration (an "options" type). Consider the situation where two instances of the type are created and need to be 
merged. For example, instance A, contains options specified via command line arguments and instance B contains options parsed from an external file. If instance A 
has precedence, instance B property values should only be considered for properties that are unset in instance A. This means that there must be some way of identifying 
a property as unset.  

In this situation, the options type has its own default property values. Checking if a property in instance A returns a default value would be an unreliable way to determine if a property is unset. This is because a default 
value could be passed as a command line argument. A solution is to use a nullable backing field for every property. If the value of a property's backing field is null, the property is considered to be 
unset.  

If such objects need to be serialized and deserialized (say for marshalling across AssemblyLoadContext boundaries), only the backing fields need to be considered. PrivateFieldsJsonConverter
can be used in such situations.

Note that the extra and missing collections are there to facilitate logging of incompatibilities between json and target type when deserializing. This is especially useful
when marshalling - oftentimes different versions of the same Type may be used on each side of a boundary.


### Behaviour
- Includes fields with null values when serializing
- Ignores most Newtonsoft.Json options (attributes etc) for now
