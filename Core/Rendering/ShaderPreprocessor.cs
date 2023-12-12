using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using Vulkan;

namespace Voxel.Core.Rendering;

/// <summary>
/// Uses a slightly off GLSL format for shaders, with some preprocessor tags and other qol added.
/// </summary>
public static class ShaderPreprocessor {
    private const string RegexExpressionTokenize = @"""([^""]*)""|[a-zA-Z0-9_]+|[#()]";

    public static void Preprocess(string source, Func<string, string> dependencyProvider, out string vertexSource, out string fragmentSource) {
        source = PreprocessIncludes(source, dependencyProvider, new());

        vertexSource = BuildVertex(source, out var vecReturnType);
        fragmentSource = BuildFragment(source, vecReturnType);
    }

    private static string BuildVertex(string source, out List<string> outputTypes) {
        source = OmitAreas(source, new HashSet<string> {
            "VERTEX"
        });
        
        outputTypes = new List<string>();
        var builder = new StringBuilder();
        builder.AppendLine("#version 450");
        var inputs = new List<ShaderFuncArg>();


        //Remove fragment shader
        var fragIndex = source.IndexOf("void frag(");
        if (fragIndex != -1) {
            var endIndex = 0;
            var parenCount = 0;
            for (endIndex = fragIndex; endIndex < source.Length; endIndex++) {
                if (source[endIndex] == '{')
                    parenCount++;
                if (source[endIndex] == '}') {
                    parenCount--;
                    endIndex++;

                    if (parenCount == 0) {
                        break;
                    }
                }
            }

            source = source.Remove(fragIndex, endIndex - fragIndex);
        }

        builder.Append(source);

        var tokens = new List<Match>();
        Tokenize(source, tokens);

        Match previous = null;
        int index = 0;
        while (ConsumeToken(ref index, out var token, tokens)) {
            if (!token.ValueSpan.SequenceEqual("vert") || !MatchToken(ref index, "(", tokens)) {
                previous = token;
                continue;
            }

            while (ConsumeToken(ref index, out var firstToken, tokens) && ConsumeToken(ref index, out var secondToken, tokens)) {

                //If closed paren, break.
                if (firstToken.ValueSpan.SequenceEqual(")"))
                    break;

                //This was an output
                if (firstToken.ValueSpan.SequenceEqual("out")) {
                    ConsumeToken(ref index, out _, tokens); //Consume name, doesn't matter for outputs.
                    outputTypes.Add(secondToken.Value);
                } else {
                    if (outputTypes.Count > 0)
                        throw new InvalidOperationException("All out variables must go at the end of the vert function inputs");

                    inputs.Add(new ShaderFuncArg {
                        type = firstToken.Value, name = secondToken.Value,
                    });
                }
            }

            break;
        }

        var argsString = new StringBuilder();

        //Input from vertex data
        for (int i = 0; i < inputs.Count; i++) {
            var input = inputs[i];
            builder.AppendLine($"layout(location = {i}) in {input.type} precomp_vs_input_{i};");

            argsString.Append($"precomp_vs_input_{i}");
            if (i < inputs.Count - 1 || outputTypes.Count != 0)
                argsString.Append(", ");
        }

        //Output to fragment shader
        builder.AppendLine();
        for (var i = 0; i < outputTypes.Count; i++) {
            var type = outputTypes[i];

            builder.AppendLine($"layout(location = {i}) out {type} precomp_v2f_transfer_{i};");
            argsString.Append($"precomp_v2f_transfer_{i}");

            if (i < outputTypes.Count - 1)
                argsString.Append(", ");
        }

        //Main function
        builder.AppendLine();
        builder.AppendLine($"void main(){{\n vert({argsString}); \n}}");

        return builder.ToString();
    }

    private static string BuildFragment(string source, List<string> inputTypes) {
        source = OmitAreas(source, new HashSet<string> {
            "FRAGMENT"
        });
        
        var builder = new StringBuilder();
        builder.AppendLine("#version 450");
        var outputs = new List<ShaderFuncArg>();

        //Remove vertex shader
        var vertIndex = source.IndexOf("void vert(");
        if (vertIndex != -1) {
            var endIndex = 0;
            var parenCount = 0;
            for (endIndex = vertIndex; endIndex < source.Length; endIndex++) {
                if (source[endIndex] == '{')
                    parenCount++;
                if (source[endIndex] == '}') {
                    parenCount--;
                    endIndex++;

                    if (parenCount == 0) {
                        break;
                    }
                }
            }

            source = source.Remove(vertIndex, endIndex - vertIndex);
        }

        builder.Append(source);

        var tokens = new List<Match>();
        Tokenize(source, tokens);

        Match previous = null;
        int index = 0;
        while (ConsumeToken(ref index, out var token, tokens)) {
            if (!token.ValueSpan.SequenceEqual("frag") || !MatchToken(ref index, "(", tokens)) {
                previous = token;
                continue;
            }

            //Match first inputs for fragment to vertex inputs.
            foreach (string inputType in inputTypes) {
                if (!MatchToken(ref index, inputType, tokens) || !ConsumeToken(ref index, out _, tokens))
                    throw new InvalidOperationException("frag function first input must be the same type as vert function return type.");
            }

            while (MatchToken(ref index, "out", tokens) && ConsumeToken(ref index, out var firstToken, tokens) && ConsumeToken(ref index, out var secondToken, tokens)) {
                outputs.Add(new ShaderFuncArg {
                    type = firstToken.Value, name = secondToken.Value,
                });
            }

            break;
        }

        var argsString = new StringBuilder();


        //Input from vertex shader
        builder.AppendLine();
        for (var i = 0; i < inputTypes.Count; i++) {
            var inputType = inputTypes[i];
            builder.AppendLine($"layout(location = {i}) in {inputType} precomp_v2f_transfer_{i};");

            argsString.Append($"precomp_v2f_transfer_{i}");
            if (i < outputs.Count - 1 || outputs.Count > 0)
                argsString.Append(", ");
        }

        builder.AppendLine();
        //Output from fragment shader.
        for (int i = 0; i < outputs.Count; i++) {
            var output = outputs[i];
            builder.AppendLine($"layout(location = {i}) out {output.type} precomp_fs_output_{i};");

            argsString.Append($"precomp_fs_output_{i}");
            if (i < outputs.Count - 1)
                argsString.Append(", ");
        }

        //main function
        builder.AppendLine();
        builder.AppendLine($"void main(){{\n frag({argsString}); \n}}");

        return builder.ToString();
    }

    private static string PreprocessIncludes(string source, Func<string, string> dependencyProvider, HashSet<string> includedFiles) {
        var tokens = new List<Match>();

        Tokenize(source, tokens);

        var includes = new List<IncludeSpan>();

        var index = 0;
        while (ConsumeToken(ref index, out var tkn, tokens)) {
            if (tkn.ValueSpan.SequenceEqual("#")) {
                if (!MatchToken(ref index, "include", tokens) || !ConsumeToken(ref index, out var nextToken, tokens))
                    break;

                var filePath = nextToken.ValueSpan.Trim('"');

                includes.Add(new IncludeSpan {
                    startIndex = tkn.Index, length = (nextToken.Index + nextToken.Length) - tkn.Index, filePath = filePath.ToString()
                });
            }
        }

        for (var i = includes.Count - 1; i >= 0; i--) {
            var include = includes[i];

            //Do not re-include already-included files
            if (includedFiles.Contains(include.filePath))
                continue;
            includedFiles.Add(include.filePath);
            var processed = PreprocessIncludes(dependencyProvider(include.filePath), dependencyProvider, includedFiles);

            //Remove include preprocessor directive.
            source = source.Remove(include.startIndex, include.length);

            //Insert the file into the original string.
            source = source.Insert(include.startIndex, processed);
        }

        return source;
    }

    public static string OmitAreas(string source, HashSet<string> valid) {
        var tokens = new List<Match>();
        Tokenize(source, tokens);

        int index = 0;

        List<IncludeSpan> areas = new();

        while (ConsumeToken(ref index, out var baseToken, tokens)) {
            //Find #AREA
            if (!baseToken.ValueSpan.SequenceEqual("#"))
                continue;
            if (!MatchToken(ref index, "AREA", tokens))
                continue;

            //Get name of area
            if (!ConsumeToken(ref index, out var nameToken, tokens))
                continue;

            //Find #END
            while (ConsumeToken(ref index, out var endToken, tokens)) {
                if (!endToken.ValueSpan.SequenceEqual("#"))
                    continue;

                if (!ConsumeToken(ref index, out var endLabel, tokens))
                    continue;

                //Always omit #AREA & label at start.
                areas.Add(new IncludeSpan {
                    startIndex = baseToken.Index, length = (nameToken.Index + nameToken.Length) - baseToken.Index, filePath = string.Empty,
                });

                //Conditionally omit area itself
                areas.Add(new IncludeSpan {
                    startIndex = nameToken.Index + nameToken.Length, length = (endToken.Index) - (nameToken.Index + nameToken.Length), filePath = nameToken.Value
                });

                //Always omit the #END at the end
                areas.Add(new IncludeSpan {
                    startIndex = endToken.Index, length = (endLabel.Index + endLabel.Length) - endToken.Index, filePath = string.Empty,
                });
            }
        }

        if (areas.Count == 0)
            return source;

        var builder = new StringBuilder();
        builder.Append(source);

        for (var i = areas.Count - 1; i >= 0; i--) {
            var area = areas[i];

            if (!valid.Contains(area.filePath))
                builder.Remove(area.startIndex, area.length);
        }

        return builder.ToString();
    }

    private static void Tokenize(string source, List<Match> toFill) {
        toFill.Clear();
        foreach (Match match in Regex.Matches(source, RegexExpressionTokenize))
            toFill.Add(match);
    }

    private static bool MatchToken(ref int index, string match, List<Match> tokens)
        => ConsumeToken(ref index, out var token, tokens) && token.ValueSpan.SequenceEqual(match);

    private static bool ConsumeToken(ref int index, [NotNullWhen(true)] out Match? token, List<Match> tokens) {
        token = null;
        if (index + 1 >= tokens.Count)
            return false;

        token = tokens[index++];
        return true;
    }


    private struct IncludeSpan {
        public int startIndex;
        public int length;

        public string filePath;
    }


    private struct ShaderFuncArg {
        public string type, name;
    }
}
