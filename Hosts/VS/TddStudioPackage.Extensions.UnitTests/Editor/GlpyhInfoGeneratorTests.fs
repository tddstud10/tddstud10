module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor.GlpyhInfoGeneratorTests

open Xunit

(*
- Given: 
  - seq<Tag> 
    - Tag is one of TST, FPT or CCT
    - The are all tags beloning to a single code line
- Needed:
  - option<{ color, glyphType [TS | FP | CCf | CCp], glyphTags }>

- If input is empty, return none
- color is
  - Red - if there are any CCT with failed TR
  - Green - if everything has passing TRs
  - White - else
- glyphType is
  - TS : If any of the tags is a TST
  - PF : If there are no TST tags and any of the tags are FPT
  - CCf : If all tags are CCT and and all have testresults
  - CCp : If all tags are CCT and any of them dont have test results
- glyphTag : carry over the union of all the tags

Tests:
- For empty input return None
- Color test - 1 TST 1 FPT 1 CCTp 1 CCTf - return Red and all GlyphTags
- Color test - 1 TST 1 FPT 1 CCTp 1 CCTp - return Green and all GlyphTags
- Color test - 1 TST 1 FPT 1 CCTx 1 CCTx - return White and all GlyphTags
- GlyphType test - 1 TST 1 FPT 1 CCTp 1 CCTf 1 CCTx - return TS and all GlyphTags
- GlyphType test - 1 FPT 1 CCTp 1 CCTf 1 CCTx - return FP and all GlyphTags
- GlyphType test - 1 CCTp 1 CCTf - return CCf and all GlyphTags
- GlyphType test - 1 CCTp 1 CCTf 1 CCTx - return CCp and all GlyphTags

 *)
