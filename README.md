To adjust the anomaly chance, you just need to look inside your EndlessManager.cs script.

Scroll down to the SetupSegment() method. Inside that method, look for this exact block of code:

C#
// THE PARANOIA MECHANIC: 
// Instead of 0% (safe), there is a rare 10% chance of a back-to-back anomaly!
else if (isFlushingBuffer) isAnomaly = Random.value < 0.10f; 

else if (normalStreak >= 2) isAnomaly = true;
else isAnomaly = Random.value < 0.35f; // Standard 35% chance
Here is exactly what you change to tweak the percentages:

To change the standard anomaly chance: Change the 0.35f. If you want a 50/50 coin toss on every normal hallway, change it to 0.50f. If you want them to be rare, change it to 0.20f.

To change the "Grace Period" chance: Change the 0.10f. If you want the immediate room after a level-up to be 100% safe every single time, change it to 0.00f. If you want it to be a 25% chance of a back-to-back jump scare, change it to 0.25f.

Just tweak those two decimals whenever you want to adjust the pacing!
