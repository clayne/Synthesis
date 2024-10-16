# Other Dynamic Mod Generators
When running with other programs that generate content, there are two styles:

## Ones that respect load order
If the program only considers content before it in the load order (Synthesis itself is in this category), then the only thing that matters is that the order you run the programs should match the order they appear in the load order

If this is your load order:
```
OtherProgramThatRespectsLoadOrder.esp
Synthesis.esp
```
Then run the other program first, followed by Synthesis.  If they're reversed, then you'd run them reversed.

## Ones that run on the entire load order
If the other program runs on the entire load order no matter what, then it is recommended to place Synthesis before that other program in the load order

```
Synthesis.esp
OtherProgramThatRunsOnEntireLoadOrder.esp
```

You would then run Synthesis first, and then the other program.   Synthesis would regenerate without considering the other program's content, and then the other program would build on top.

## Specific Recommendations
### Bashed Patch
Probably good to run before Synthesis in the load order.  It processes leveled lists and might undesirably revert changes made by Synthesis patchers depending on the contexts.
