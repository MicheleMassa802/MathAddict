# How does MA behave?

### What does it look like when you enter the website:
- In main dashboard the url is `https://mathacademy.com/learn`.
  - This would be the trigger to pop up the extension and ask the user to start up a session.

### What does it look like when you are in a lesson:
- When in a lesson you are on an url like: `https://mathacademy.com/tasks/5444256/topics/1117/lesson`
  - Whenever the user submits their answer, the url doesn't change because you are on the same lesson until you finish it.
  - You get one of the two outcomes (shown on the question below) for the current question box you are on.
- => When on this lesson page, you show the slots machine on standby.

### What are the outcomes of a question: 
- When you get the correct answer, you get the in-page message (not a pop-up) seen on `MathAddictChromium/Misc/SampleCorrect.html`
  - This allows you to go to the next question, but you'd only be able to detect this by knowing the current question # with the submit button (shown on MathAddictChromium/Misc/QuestionWidget.html) and seeing when the question state changes.
    - Maybe we inject the onclick for the current question button? => This would trigger a roll!
  - After the correct answer you get a continue button to load and show the next one.
- When you get an incorrect answer, you get the in-page message (not a pop-up) seen on `MathAddictChromium/Misc/SampleIncorrect.html`
  - This makes you go to the next question, no chance for re-try => This would trigger a no roll!

### What happens when you exit / complete a lesson:
- If you go back to the dashboard => the link changes so you can go back to the non-slots visual.
- If you complete a lesson, when you click the last `continue` button => you get the screen shown on `MathAddictChromium/Misc/LessonComplete.html`.
  - Clicking on `continue` leads to the dashboard screen => so you can go back to the non-slots visual.

