#  ***SLang** Visual Studio Extension*
## Feature Support Documentation

Add support for SLang Programming Language into Visual Studio IDE.
Target version is Visual Studio 2019.

#### Essential Features:
- [ ] **Code Highlight (Tagger)**
    - [x] Basic support
    - [ ] Interface with parser
- [ ] **Project management**
    - [x] Create project file (\*.slangproj)
    - [ ] Templates (Empty project, Basic Command Line app)
    - [ ] Build with compiler (output paths, debug/release modes)
    - [ ] Settings (compiler path, startup/other file)
- [ ] **IntelliSense (define api, write stubs)**
    - [x] Quick Info (semantic api)
    - [x] Error indication (semantic api)
    - [ ] List Members (semantic api)
    - [ ] Parameter Info / Signature Help (semantic api)
    - [ ] Complete Word (semantic api)
    - [ ] Lightbulb suggestions (semantic api)
- [ ] **Context Menu Commands**
    - [ ] Go to definition (semantic api)
    - [ ] Peek definition (create wpf window view?)(semantic api)
    - [ ] Find all references (semantic api)
- [x] **Code folding (Tagger)(Outlining) + (needs to be moved to AST interface)**
- [ ] `.editorconfig` **features**
    - [ ] explicit integration into project
    - [ ] hidden integration
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
- Snippets
---

### Custom Coloring rules
|Symbol|Default Color|In Hex|
|-|-|-|
|Undefined symbol|Dark gray||
|Unit definition|Light teal|<span style="color:#4ec9b0;background-color:black;">#4ec9b0</span>|
|Unit member routine definition|Light yellow|<span style="color:#dcdcaa;background-color:black;">#dcdcaa</span>|
|||

