using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace WSSTest {
    internal class MusicReader {
        MidiFile midiFile = MidiFile.Read("C:\\Users\\3vilpcdiva\\Documents\\test.mid");
        public List<KeyValuePair<int, int>> ReadMusic() {
            TempoMap tm = midiFile.GetTempoMap();
            var TicksPerQuarterNote = tm.TimeDivision;
            TicksPerQuarterNoteTimeDivision t = (TicksPerQuarterNoteTimeDivision)TicksPerQuarterNote;
            Console.WriteLine($"Ticks Per Quarter Note: " + t.TicksPerQuarterNote);
            MetricTimeSpan metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(t.TicksPerQuarterNote, tm);
            //Console.WriteLine("Metric Time Span for one quarter note: " + metricTimeSpan);
            List<KeyValuePair<int, int>> noteInfo = new List<KeyValuePair<int, int>>();
            var notes = midiFile.GetNotes();
            /*
             * for each note in notes get the start time and duration
             * then convert the duration to milliseconds
             * then create a list of key value pairs where the key is the note number and the value is the duration in milliseconds 
             * create notes with value of 0 that have a duration lasting from the end of each note to the start of the next note
             * then create the list of key value pairs where the key is the note number and the value is the duration in milliseconds
             * 
             */
            long? prevEndTicks = null;
            foreach ( var note in notes ) {
                long startTicks = note.Time;
                long endTicks = note.EndTime;
               Console.WriteLine($"Note {note.NoteNumber} starts at {startTicks} and ends at {endTicks}");
                if(prevEndTicks.HasValue && startTicks > prevEndTicks.Value) {
                    long restTicks = startTicks - prevEndTicks.Value;
                    var restMetricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(restTicks, tm);
                    int restDurationMs = (int)(restMetricTimeSpan.Minutes * 60000 + restMetricTimeSpan.Seconds * 1000 + restMetricTimeSpan.Milliseconds);
                    if(restDurationMs > 0) {
                        noteInfo.Add(new KeyValuePair<int, int>(0, restDurationMs));
                    }
                }
                var startMetricForNote = TimeConverter.ConvertTo<MetricTimeSpan>(startTicks, tm);
                var endMetricForNote = TimeConverter.ConvertTo<MetricTimeSpan>(endTicks, tm);
                var durationMs = (int)Math.Max(1, (endMetricForNote.TotalMicroseconds - startMetricForNote.TotalMicroseconds) / 1000);
                noteInfo.Add(new KeyValuePair<int, int>(note.NoteNumber, durationMs));
                prevEndTicks = endTicks;

            }



            return noteInfo;
        }
    }

}

