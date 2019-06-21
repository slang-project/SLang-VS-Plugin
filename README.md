#  ***SLang** Visual Studio Extension*
## Feature Support Documentation

Add support for SLang Programming Language into Visual Studio IDE.
Target version is Visual Studio 2019.

#### Essential Features:
- [ ] **Code Highlight (Tagger)**
    - [x] Lexer-based
    - [ ] Semantic additions (semantic api)
- [x] **Project management**
    - [x] Create project file (\*.slangproj)
    - [x] Templates (Empty project, Basic Command Line app)
    - [x] Build with compiler (output paths, debug/release modes)
    - [x] Settings (compiler path, startup/other file)
- [ ] **IntelliSense (define api, write stubs)**
    - [x] Quick Info (semantic api)
    - [x] Error indication (semantic api)
    - [ ] List Members (semantic api)
    - [ ] Parameter Info / Signature Help (semantic api)
    - [ ] Complete Word (semantic api)
    - [ ] Lightbulb suggestions (semantic api)
- [ ] **Context Menu Commands**
    - [x] Go to definition (semantic api)
    - [ ] Peek definition (create wpf window view?)(semantic api)
    - [ ] Find all references (semantic api)
- [x] **Code folding (Outlining) + (needs to be moved to AST interface)**
- [x] `.editorconfig` **features**
    - [x] explicit integration into project
- [x] **Comment/uncomment commands + (multi-line/doc comment in future)**
- [ ] **Brace matching in expressions**
    - [ ] variadic colorization
- [ ] **Occurances matching (semantic api)**
- [ ] **Smart Indent**


#### Future Consideration Features:
- Analysis tools (maybe slang metrics?)
- Document formatting (need separate formatter of some customizable tool)
- IntelliCode (requires deep learning)
- Debugger
    - debug mode laungh
    - breakpoints
    - variable watches
- Navigation Bars (function/etc navigation dropdown on top of editor)
- Solution Explorer Search
- Snippets
---

### Custom Coloring rules
|Symbol|Default Color|In Hex|
|-|-|-|
|Undefined symbol|Dark gray||
|Unit definition|Light Teal|#4ec9b0|
|Unit member routine definition|Light Yellow|#dcdcaa|
|||

