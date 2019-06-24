#  ***SLang** Visual Studio Extension*
## Feature Support Documentation

Add support for SLang Programming Language into Visual Studio IDE.
Target version is Visual Studio 2019.

Up-to-date development progress can be tracked via ["Essential Features" github project](https://github.com/slang-project/SLang-VS-Plugin/projects/1)

#### Implemented Features:
- [ ] **Code Highlight (Tagger)**
    - [x] Lexer-based
- [ ] **Symbol Resolver**
    - [ ] Resolver Tagger
- [x] **Project management**
    - [x] Create project file (\*.slangproj)
    - [x] Templates (Empty project, Basic Command Line app)
    - [x] Build with compiler (output paths, debug/release modes)
    - [x] Settings (compiler path, startup/other file)
- [ ] **IntelliSense (define api, write stubs)**
    - [x] Quick Info (file context)
    - [x] Error indication (interface only)
    - [x] List Members (plain file context)
- [x] **Code Navigation**
    - [x] Go to definition (file context)
- [x] **Code folding (Outlining) + (needs to be moved to AST interface)**
- [x] `.editorconfig`
    - [x] explicit integration into project
- [x] **Comment/uncomment commands**
- [x] **Brace matching in expressions**
- [x] **Occurance matching (Plain file context)**
- [ ] **Smart/Auto Indent**
- [ ] **Indent Guides**


#### Future Consideration Features:
- Analysis tools (maybe slang metrics?)
- Document formatting (need separate formatter of some customizable tool)
- IntelliCode (requires deep learning)
- Debugger
    - Debug mode launch
    - breakpoints
    - variable watches
- Navigation Bars (function/etc navigation dropdown on top of editor)
- Solution Explorer Search
- Snippets
- Occurance Matching (semantic api)
- Intellisense features
    - Parameter Info / Signature Help (semantic api)
    - Complete Word (semantic api)
    - Lightbulb suggestions (semantic api)
    - Error indication (senabtic api)
    - List Members (semantic api)
- Code Navigation
    - Peek definition (create wpf window view?)(semantic api)
    - Find all references (semantic api)
---

### Custom Coloring rules
|Symbol|Default Color|In Hex|
|-|-|-|
|Undefined symbol|Dark gray||
|Unit definition|Light Teal|#4ec9b0|
|Unit member routine definition|Light Yellow|#dcdcaa|
|||

