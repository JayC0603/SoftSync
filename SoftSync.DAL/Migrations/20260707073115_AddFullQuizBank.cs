using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SoftSync.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddFullQuizBank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.InsertData(
                table: "AssessmentQuestions",
                columns: new[] { "Id", "QuestionText", "SkillId", "Type" },
                values: new object[,]
                {
                    { 101, "A teammate interrupts you mid-sentence in a meeting. What do you do?", 1, 1 },
                    { 102, "How important is active listening in a conversation?", 1, 0 },
                    { 103, "You need to explain a complex idea to a non-expert. You...", 1, 1 },
                    { 104, "When giving feedback to a peer, you tend to...", 1, 0 },
                    { 105, "You disagree with a decision in a group chat. You...", 1, 1 },
                    { 106, "How do you handle a message you don't fully understand?", 1, 0 },
                    { 107, "You're presenting and notice the audience looks confused. You...", 1, 1 },
                    { 108, "In written communication, you...", 1, 0 },
                    { 201, "A team member isn't contributing to a group project. You...", 2, 1 },
                    { 202, "When your idea conflicts with a teammate's, you...", 2, 0 },
                    { 203, "The team hits a setback close to a deadline. You...", 2, 1 },
                    { 204, "How do you treat credit for a shared success?", 2, 0 },
                    { 205, "A quieter teammate hasn't shared their opinion. You...", 2, 1 },
                    { 206, "When you receive a task you dislike but the team needs, you...", 2, 0 },
                    { 207, "Two teammates are in conflict and it's slowing work. You...", 2, 1 },
                    { 208, "How reliable are you with commitments to the team?", 2, 0 },
                    { 301, "You realize you will miss a deadline tomorrow. Your first action is to...", 3, 1 },
                    { 302, "How do you start your workday?", 3, 0 },
                    { 303, "You have three tasks due and can't finish all. You...", 3, 1 },
                    { 304, "How do you handle distractions while working?", 3, 0 },
                    { 305, "A new urgent request lands mid-task. You...", 3, 1 },
                    { 306, "How well do you estimate how long tasks take?", 3, 0 },
                    { 307, "You keep procrastinating on a big task. You...", 3, 1 },
                    { 308, "At the end of the week you...", 3, 0 },
                    { 401, "You read a surprising statistic online. You...", 4, 0 },
                    { 402, "A popular solution is proposed for a problem. You...", 4, 1 },
                    { 403, "When you form an opinion, you...", 4, 0 },
                    { 404, "Two experts give you conflicting advice. You...", 4, 1 },
                    { 405, "How do you treat your own assumptions?", 4, 0 },
                    { 406, "Data shows your favorite approach is underperforming. You...", 4, 1 },
                    { 407, "Faced with a complex claim, you first...", 4, 0 },
                    { 408, "How do you distinguish correlation from causation?", 4, 0 },
                    { 501, "You hit an unfamiliar problem with no obvious solution. You...", 5, 1 },
                    { 502, "When a solution fails, you...", 5, 0 },
                    { 503, "A problem is too big to tackle at once. You...", 5, 1 },
                    { 504, "How do you generate solution ideas?", 5, 0 },
                    { 505, "You lack information to solve a problem. You...", 5, 1 },
                    { 506, "How do you validate that a solution actually works?", 5, 0 },
                    { 507, "Your solution works but is inefficient. You...", 5, 1 },
                    { 508, "When stuck, you...", 5, 0 },
                    { 601, "You receive harsh criticism on your work. You...", 6, 1 },
                    { 602, "When you feel angry at work, you...", 6, 0 },
                    { 603, "A stressful deadline is making you anxious. You...", 6, 1 },
                    { 604, "How aware are you of your emotions as they happen?", 6, 0 },
                    { 605, "A colleague is visibly upset. You...", 6, 1 },
                    { 606, "After a setback, how do you recover?", 6, 0 },
                    { 607, "You're frustrated in a meeting. You...", 6, 1 },
                    { 608, "How do you handle others' strong emotions?", 6, 0 },
                    { 701, "Your project's requirements change suddenly. You...", 7, 1 },
                    { 702, "How do you feel about learning new tools or methods?", 7, 0 },
                    { 703, "You're moved to an unfamiliar team overnight. You...", 7, 1 },
                    { 704, "When plans fall apart, your mindset is...", 7, 0 },
                    { 705, "A tool you rely on is discontinued. You...", 7, 1 },
                    { 706, "How do you respond to feedback that you should change how you work?", 7, 0 },
                    { 707, "Priorities shift for the third time this week. You...", 7, 1 },
                    { 708, "In an uncertain, ambiguous situation you...", 7, 0 }
                });

            migrationBuilder.InsertData(
                table: "AssessmentOptions",
                columns: new[] { "Id", "OptionText", "QuestionId", "ScoreValue" },
                values: new object[,]
                {
                    { 1011, "Politely note you'd like to finish, then continue your point.", 101, 4 },
                    { 1012, "Stop talking and let the moment go.", 101, 2 },
                    { 1013, "Raise your voice to talk over them.", 101, 1 },
                    { 1014, "Say nothing but complain about them later.", 101, 1 },
                    { 1021, "Essential — I paraphrase to confirm I understood.", 102, 4 },
                    { 1022, "Fairly important, I try to listen most of the time.", 102, 3 },
                    { 1023, "Somewhat — I mostly wait for my turn to talk.", 102, 2 },
                    { 1024, "Not important.", 102, 1 },
                    { 1031, "Use plain language and a relatable example.", 103, 4 },
                    { 1032, "Explain it fully with all the technical terms.", 103, 2 },
                    { 1033, "Send them a document to read on their own.", 103, 2 },
                    { 1034, "Assume they'll figure it out.", 103, 1 },
                    { 1041, "Be specific, kind, and focus on the behavior.", 104, 4 },
                    { 1042, "Give general praise and avoid the hard parts.", 104, 2 },
                    { 1043, "Point out only what went wrong.", 104, 2 },
                    { 1044, "Avoid giving feedback at all.", 104, 1 },
                    { 1051, "Share your view calmly with reasons and ask for others'.", 105, 4 },
                    { 1052, "Go along with it to keep the peace.", 105, 2 },
                    { 1053, "Send a blunt message showing your frustration.", 105, 1 },
                    { 1054, "Stay silent and disengage.", 105, 1 },
                    { 1061, "Ask a clarifying question right away.", 106, 4 },
                    { 1062, "Guess the meaning and reply.", 106, 2 },
                    { 1063, "Wait and hope it becomes clear later.", 106, 2 },
                    { 1064, "Ignore it.", 106, 1 },
                    { 1071, "Pause, check in, and re-explain the key point.", 107, 4 },
                    { 1072, "Slow down but keep going as planned.", 107, 3 },
                    { 1073, "Speed up to finish sooner.", 107, 1 },
                    { 1074, "Ignore it and push through.", 107, 1 },
                    { 1081, "Adapt tone and detail to the reader.", 108, 4 },
                    { 1082, "Keep it short regardless of the reader.", 108, 2 },
                    { 1083, "Write the same way for everyone.", 108, 2 },
                    { 1084, "Don't think about tone.", 108, 1 },
                    { 2011, "Talk to them privately to understand what's going on.", 201, 4 },
                    { 2012, "Quietly do their share yourself.", 201, 2 },
                    { 2013, "Report them to the supervisor immediately.", 201, 2 },
                    { 2014, "Call them out in front of the group.", 201, 1 },
                    { 2021, "Look for a solution that combines the best of both.", 202, 4 },
                    { 2022, "Push for your idea because you're confident in it.", 202, 2 },
                    { 2023, "Give up your idea to avoid friction.", 202, 2 },
                    { 2024, "Refuse to work with them.", 202, 1 },
                    { 2031, "Rally the group, re-plan, and share the load.", 203, 4 },
                    { 2032, "Focus only on finishing your own part.", 203, 2 },
                    { 2033, "Wait for someone else to take charge.", 203, 2 },
                    { 2034, "Blame whoever caused it.", 203, 1 },
                    { 2041, "Acknowledge everyone's contribution.", 204, 4 },
                    { 2042, "Mention the team if asked.", 204, 3 },
                    { 2043, "Highlight my own part first.", 204, 2 },
                    { 2044, "Take the credit myself.", 204, 1 },
                    { 2051, "Invite them in and ask what they think.", 205, 4 },
                    { 2052, "Assume they agree with the group.", 205, 2 },
                    { 2053, "Move on without them.", 205, 2 },
                    { 2054, "Decide for them.", 205, 1 },
                    { 2061, "Take it on and do it well for the team.", 206, 4 },
                    { 2062, "Do it, but with minimal effort.", 206, 2 },
                    { 2063, "Try to pass it to someone else.", 206, 2 },
                    { 2064, "Refuse.", 206, 1 },
                    { 2071, "Help them talk it through and find common ground.", 207, 4 },
                    { 2072, "Pick the side you agree with.", 207, 2 },
                    { 2073, "Stay out of it entirely.", 207, 2 },
                    { 2074, "Tell everyone to just get over it.", 207, 1 },
                    { 2081, "I deliver what I promise, on time, consistently.", 208, 4 },
                    { 2082, "Usually reliable, occasional slips.", 208, 3 },
                    { 2083, "Reliable only when reminded.", 208, 2 },
                    { 2084, "Often miss commitments.", 208, 1 },
                    { 3011, "Inform stakeholders and propose a new timeline.", 301, 4 },
                    { 3012, "Work all night and hope it's enough.", 301, 2 },
                    { 3013, "Say nothing and hand it in late.", 301, 1 },
                    { 3014, "Ask for the deadline to be dropped.", 301, 2 },
                    { 3021, "Review priorities and plan the day.", 302, 4 },
                    { 3022, "Jump into whatever feels urgent.", 302, 2 },
                    { 3023, "Check messages first, then wing it.", 302, 2 },
                    { 3024, "Start with the easiest tasks.", 302, 2 },
                    { 3031, "Prioritize by impact and deadline, then focus.", 303, 4 },
                    { 3032, "Do them in the order they arrived.", 303, 2 },
                    { 3033, "Do the quickest ones and leave the big one.", 303, 2 },
                    { 3034, "Try to multitask all three at once.", 303, 1 },
                    { 3041, "Block focus time and silence interruptions.", 304, 4 },
                    { 3042, "Take breaks whenever a distraction appears.", 304, 2 },
                    { 3043, "Try to resist but often give in.", 304, 2 },
                    { 3044, "I'm easily pulled off-task.", 304, 1 },
                    { 3051, "Assess its true priority before switching.", 305, 4 },
                    { 3052, "Drop everything and switch immediately.", 305, 2 },
                    { 3053, "Ignore it until my current task is done.", 305, 2 },
                    { 3054, "Panic and lose track of both.", 305, 1 },
                    { 3061, "Fairly accurately, with buffer for surprises.", 306, 4 },
                    { 3062, "Roughly right most of the time.", 306, 3 },
                    { 3063, "I usually underestimate.", 306, 2 },
                    { 3064, "I don't estimate at all.", 306, 1 },
                    { 3071, "Break it into small steps and start one now.", 307, 4 },
                    { 3072, "Wait until you feel motivated.", 307, 2 },
                    { 3073, "Do smaller tasks to feel productive.", 307, 2 },
                    { 3074, "Keep putting it off.", 307, 1 },
                    { 3081, "Reflect on what worked and adjust next week.", 308, 4 },
                    { 3082, "Just move on to the weekend.", 308, 2 },
                    { 3083, "Feel behind but don't review why.", 308, 2 },
                    { 3084, "Rarely think about it.", 308, 1 },
                    { 4011, "Check the source and how the data was gathered.", 401, 4 },
                    { 4012, "Trust it if it sounds reasonable.", 401, 2 },
                    { 4013, "Share it if it matches what I believe.", 401, 1 },
                    { 4014, "Accept it because it was widely posted.", 401, 2 },
                    { 4021, "Weigh evidence and consider alternatives first.", 402, 4 },
                    { 4022, "Go with it since everyone agrees.", 402, 2 },
                    { 4023, "Support it to avoid standing out.", 402, 2 },
                    { 4024, "Reject it just to be different.", 402, 1 },
                    { 4031, "Actively seek views that challenge mine.", 403, 4 },
                    { 4032, "Consider other views if they come up.", 403, 3 },
                    { 4033, "Mostly look for support for my view.", 403, 2 },
                    { 4034, "Rarely question my first reaction.", 403, 1 },
                    { 4041, "Examine the reasoning and evidence behind each.", 404, 4 },
                    { 4042, "Follow whoever is more senior.", 404, 2 },
                    { 4043, "Pick the advice that's easier to follow.", 404, 2 },
                    { 4044, "Get stuck and do nothing.", 404, 1 },
                    { 4051, "I identify and test them explicitly.", 405, 4 },
                    { 4052, "I notice them sometimes.", 405, 3 },
                    { 4053, "I rarely question them.", 405, 2 },
                    { 4054, "I don't think about them.", 405, 1 },
                    { 4061, "Accept the data and revise the approach.", 406, 4 },
                    { 4062, "Look for reasons to dismiss the data.", 406, 2 },
                    { 4063, "Keep going and hope it improves.", 406, 2 },
                    { 4064, "Ignore the data.", 406, 1 },
                    { 4071, "Break it into parts and check each.", 407, 4 },
                    { 4072, "Judge it as a whole by gut feel.", 407, 2 },
                    { 4073, "Ask what others think of it.", 407, 2 },
                    { 4074, "Accept or reject it quickly.", 407, 1 },
                    { 4081, "I look for confounders and other explanations.", 408, 4 },
                    { 4082, "I'm cautious but not always sure.", 408, 3 },
                    { 4083, "I often assume one causes the other.", 408, 2 },
                    { 4084, "I don't consider the difference.", 408, 1 },
                    { 5011, "Define the problem clearly, then explore options.", 501, 4 },
                    { 5012, "Try the first idea that comes to mind.", 501, 2 },
                    { 5013, "Wait for someone else to solve it.", 501, 1 },
                    { 5014, "Avoid it and work on something else.", 501, 1 },
                    { 5021, "Analyze why, then try a different approach.", 502, 4 },
                    { 5022, "Repeat it hoping for a different result.", 502, 2 },
                    { 5023, "Give up on that problem.", 502, 1 },
                    { 5024, "Blame external factors.", 502, 1 },
                    { 5031, "Break it into smaller, solvable pieces.", 503, 4 },
                    { 5032, "Attack the whole thing head-on.", 503, 2 },
                    { 5033, "Wait until it becomes urgent.", 503, 2 },
                    { 5034, "Hope it resolves itself.", 503, 1 },
                    { 5041, "Brainstorm several, then evaluate trade-offs.", 504, 4 },
                    { 5042, "Go with the most familiar option.", 504, 2 },
                    { 5043, "Copy what worked elsewhere without adapting.", 504, 2 },
                    { 5044, "Pick the first workable idea.", 504, 2 },
                    { 5051, "Identify what's missing and go find it.", 505, 4 },
                    { 5052, "Make assumptions and proceed.", 505, 2 },
                    { 5053, "Solve a different, easier problem instead.", 505, 2 },
                    { 5054, "Stop until someone hands you the info.", 505, 1 },
                    { 5061, "Test it against real criteria and edge cases.", 506, 4 },
                    { 5062, "Check that it looks right.", 506, 2 },
                    { 5063, "Assume it works if there's no error.", 506, 2 },
                    { 5064, "I don't validate.", 506, 1 },
                    { 5071, "Ship it, then plan a cleaner improvement.", 507, 4 },
                    { 5072, "Leave it as-is forever.", 507, 2 },
                    { 5073, "Scrap it and restart from zero.", 507, 2 },
                    { 5074, "Ignore the inefficiency.", 507, 1 },
                    { 5081, "Step back and reframe the problem.", 508, 4 },
                    { 5082, "Keep pushing the same way harder.", 508, 2 },
                    { 5083, "Take a guess and move on.", 508, 2 },
                    { 5084, "Abandon the task.", 508, 1 },
                    { 6011, "Take a breath, look for the useful part, and respond calmly.", 601, 4 },
                    { 6012, "Feel defensive and argue back.", 601, 2 },
                    { 6013, "Shut down and stop engaging.", 601, 1 },
                    { 6014, "Pretend it didn't bother you but stew on it.", 601, 2 },
                    { 6021, "Recognize it, pause, and choose how to respond.", 602, 4 },
                    { 6022, "Try to hide it but it leaks out.", 602, 2 },
                    { 6023, "Express it immediately.", 602, 1 },
                    { 6024, "Bottle it up completely.", 602, 2 },
                    { 6031, "Use a coping routine and focus on the next step.", 603, 4 },
                    { 6032, "Push through while ignoring the stress.", 603, 2 },
                    { 6033, "Let the anxiety stall your work.", 603, 1 },
                    { 6034, "Vent to everyone around you.", 603, 2 },
                    { 6041, "Very — I can name what I feel and why.", 604, 4 },
                    { 6042, "Somewhat aware in the moment.", 604, 3 },
                    { 6043, "I notice only after the fact.", 604, 2 },
                    { 6044, "Rarely aware.", 604, 1 },
                    { 6051, "Acknowledge how they feel and offer support.", 605, 4 },
                    { 6052, "Give practical advice right away.", 605, 3 },
                    { 6053, "Avoid them to not make it worse.", 605, 2 },
                    { 6054, "Tell them to calm down.", 605, 1 },
                    { 6061, "Process it, learn, and move forward.", 606, 4 },
                    { 6062, "Distract myself and avoid it.", 606, 2 },
                    { 6063, "Dwell on it for a long time.", 606, 2 },
                    { 6064, "Let it affect everything else.", 606, 1 },
                    { 6071, "Regulate your tone and stay constructive.", 607, 4 },
                    { 6072, "Go quiet and disengage.", 607, 2 },
                    { 6073, "Let the frustration show sharply.", 607, 1 },
                    { 6074, "Make a sarcastic comment.", 607, 1 },
                    { 6081, "Stay calm and help de-escalate.", 608, 4 },
                    { 6082, "Match their energy without thinking.", 608, 2 },
                    { 6083, "Withdraw from the situation.", 608, 2 },
                    { 6084, "Get overwhelmed.", 608, 1 },
                    { 7011, "Re-assess, adjust the plan, and move forward.", 701, 4 },
                    { 7012, "Resist the change and argue for the old plan.", 701, 2 },
                    { 7013, "Feel stuck and wait for direction.", 701, 2 },
                    { 7014, "Keep working on the outdated plan.", 701, 1 },
                    { 7021, "I seek them out and enjoy the challenge.", 702, 4 },
                    { 7022, "I'll learn them when required.", 702, 3 },
                    { 7023, "I prefer to stick with what I know.", 702, 2 },
                    { 7024, "I avoid change whenever possible.", 702, 1 },
                    { 7031, "Stay open, ask questions, and adapt quickly.", 703, 4 },
                    { 7032, "Wait for everything to be explained.", 703, 2 },
                    { 7033, "Compare it unfavorably to the old team.", 703, 2 },
                    { 7034, "Disengage until things settle.", 703, 1 },
                    { 7041, "An opportunity to find a better path.", 704, 4 },
                    { 7042, "An annoyance I have to tolerate.", 704, 2 },
                    { 7043, "A reason to feel discouraged.", 704, 2 },
                    { 7044, "A disaster I can't handle.", 704, 1 },
                    { 7051, "Explore alternatives and learn a replacement.", 705, 4 },
                    { 7052, "Keep using it until it fully breaks.", 705, 2 },
                    { 7053, "Wait for someone to choose a replacement.", 705, 2 },
                    { 7054, "Complain and delay adapting.", 705, 1 },
                    { 7061, "Consider it seriously and try adjusting.", 706, 4 },
                    { 7062, "Consider it but rarely change.", 706, 2 },
                    { 7063, "Feel criticized and defend my way.", 706, 2 },
                    { 7064, "Dismiss it.", 706, 1 },
                    { 7071, "Stay flexible and re-focus on what matters now.", 707, 4 },
                    { 7072, "Get frustrated but comply.", 707, 2 },
                    { 7073, "Push back on every change.", 707, 2 },
                    { 7074, "Lose motivation.", 707, 1 },
                    { 7081, "Stay calm and take sensible next steps.", 708, 4 },
                    { 7082, "Wait for full certainty before acting.", 708, 2 },
                    { 7083, "Feel paralyzed by the unknown.", 708, 2 },
                    { 7084, "Act rashly without thinking.", 708, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1011);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1012);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1013);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1014);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1021);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1022);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1023);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1024);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1031);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1032);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1033);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1034);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1041);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1042);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1043);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1044);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1051);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1052);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1053);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1054);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1061);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1062);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1063);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1064);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1071);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1072);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1073);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1074);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1081);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1082);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1083);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1084);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2011);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2012);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2013);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2014);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2021);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2022);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2023);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2024);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2031);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2032);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2033);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2034);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2041);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2042);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2043);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2044);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2051);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2052);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2053);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2054);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2061);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2062);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2063);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2064);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2071);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2072);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2073);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2074);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2081);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2082);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2083);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2084);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3011);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3012);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3013);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3014);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3021);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3022);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3023);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3024);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3031);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3032);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3033);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3034);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3041);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3042);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3043);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3044);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3051);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3052);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3053);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3054);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3061);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3062);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3063);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3064);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3071);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3072);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3073);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3074);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3081);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3082);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3083);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3084);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4011);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4012);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4013);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4014);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4021);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4022);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4023);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4024);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4031);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4032);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4033);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4034);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4041);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4042);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4043);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4044);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4051);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4052);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4053);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4054);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4061);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4062);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4063);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4064);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4071);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4072);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4073);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4074);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4081);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4082);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4083);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4084);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5011);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5012);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5013);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5014);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5021);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5022);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5023);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5024);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5031);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5032);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5033);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5034);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5041);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5042);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5043);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5044);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5051);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5052);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5053);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5054);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5061);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5062);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5063);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5064);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5071);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5072);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5073);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5074);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5081);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5082);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5083);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5084);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6011);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6012);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6013);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6014);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6021);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6022);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6023);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6024);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6031);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6032);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6033);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6034);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6041);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6042);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6043);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6044);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6051);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6052);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6053);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6054);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6061);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6062);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6063);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6064);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6071);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6072);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6073);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6074);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6081);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6082);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6083);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6084);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7011);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7012);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7013);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7014);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7021);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7022);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7023);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7024);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7031);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7032);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7033);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7034);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7041);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7042);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7043);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7044);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7051);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7052);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7053);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7054);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7061);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7062);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7063);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7064);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7071);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7072);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7073);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7074);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7081);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7082);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7083);

            migrationBuilder.DeleteData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7084);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 108);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 201);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 202);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 203);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 204);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 205);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 206);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 207);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 208);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 301);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 302);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 303);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 304);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 305);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 306);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 307);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 308);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 401);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 402);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 403);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 404);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 405);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 406);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 407);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 408);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 501);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 502);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 503);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 504);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 505);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 506);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 507);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 508);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 601);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 602);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 603);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 604);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 605);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 606);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 607);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 608);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 701);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 702);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 703);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 704);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 705);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 706);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 707);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 708);

            migrationBuilder.InsertData(
                table: "AssessmentQuestions",
                columns: new[] { "Id", "QuestionText", "SkillId", "Type" },
                values: new object[,]
                {
                    { 1, "How do you handle a conflict during a group discussion?", 1, 1 },
                    { 2, "How important is active listening in a conversation?", 1, 0 }
                });

            migrationBuilder.InsertData(
                table: "AssessmentOptions",
                columns: new[] { "Id", "OptionText", "QuestionId", "ScoreValue" },
                values: new object[,]
                {
                    { 1, "Stay quiet to avoid more conflict.", 1, 1 },
                    { 2, "Listen to all sides and suggest a compromise.", 1, 5 },
                    { 3, "Insist on my point of view.", 1, 2 },
                    { 4, "Not important.", 2, 1 },
                    { 5, "Very important.", 2, 5 }
                });
        }
    }
}
