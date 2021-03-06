﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-tools/master/LICENCE

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using osu.Game.Scoring;

namespace PerformanceCalculator.Performance
{
    public class PerformanceProcessor : IProcessor
    {
        private readonly PerformanceCommand command;

        public PerformanceProcessor(PerformanceCommand command)
        {
            this.command = command;
        }

        public void Execute()
        {
            var workingBeatmap = new ProcessorWorkingBeatmap(command.Beatmap);
            var scoreParser = new ProcessorScoreParser(workingBeatmap);

            foreach (var f in command.Replays)
            {
                Score score;
                using (var stream = File.OpenRead(f))
                    score = scoreParser.Parse(stream);

                workingBeatmap.Mods.Value = score.ScoreInfo.Mods;

                // Convert + process beatmap
                var categoryAttribs = new Dictionary<string, double>();
                double pp = score.ScoreInfo.Ruleset.CreateInstance().CreatePerformanceCalculator(workingBeatmap, score.ScoreInfo).Calculate(categoryAttribs);

                command.Console.WriteLine(f);
                writeAttribute("Player", score.ScoreInfo.User.Username);
                writeAttribute("Mods", score.ScoreInfo.Mods.Length > 0
                    ? score.ScoreInfo.Mods.Select(m => m.Acronym).Aggregate((c, n) => $"{c}, {n}")
                    : "None");

                foreach (var kvp in categoryAttribs)
                    writeAttribute(kvp.Key, kvp.Value.ToString(CultureInfo.InvariantCulture));

                writeAttribute("pp", pp.ToString(CultureInfo.InvariantCulture));
                command.Console.WriteLine();
            }
        }

        private void writeAttribute(string name, string value) => command.Console.WriteLine($"{name.PadRight(15)}: {value}");
    }
}
