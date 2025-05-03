const express = require("express");
const bodyParser = require("body-parser");
const cors = require("cors");
const mongoose = require("mongoose");
require("dotenv").config(); // Load environment variables

const app = express();
const PORT = 3000;

// Middleware
app.use(bodyParser.json());
app.use(cors({
  origin: "*", // Allow all origins for testing purposes
  methods: ["GET", "POST"],
  allowedHeaders: ["Content-Type"]
}));

// MongoDB connection
const mongoURI = process.env.MONGO_URI; // Use the connection string from .env
mongoose.connect(mongoURI, {
  useNewUrlParser: true,
  useUnifiedTopology: true,
});
const db = mongoose.connection;
db.on("error", console.error.bind(console, "MongoDB connection error:"));
db.once("open", () => {
  console.log("Connected to MongoDB Atlas");
});

// Define a schema and model for high scores
const highScoreSchema = new mongoose.Schema({
  playerName: String,
  score: Number,
});
const HighScore = mongoose.model("HighScore", highScoreSchema);

// Routes
app.get("/api/highscores", async (req, res) => {
  console.log(`[${new Date().toISOString()}] GET /api/highscores request received from ${req.ip}`); // Add logging
  try {
    const highScores = await HighScore.find().sort({ score: -1 }).limit(10); // Changed limit to 10
    res.json({ highScores });
  } catch (err) {
    console.error(`[${new Date().toISOString()}] Error fetching high scores:`, err);
    res.status(500).json({ error: "Failed to fetch high scores" });
  }
});

app.post("/api/highscores", async (req, res) => {
  console.log(`[${new Date().toISOString()}] POST /api/highscores request received from ${req.ip} with body:`, req.body);
  const { playerName, score } = req.body;

  // Input validation
  if (!playerName || typeof score !== "number" || !Number.isInteger(score) || score < 0) {
    console.warn(`[${new Date().toISOString()}] Invalid input for POST /api/highscores:`, req.body);
    return res.status(400).json({ error: "Invalid input: playerName (string) and score (non-negative integer) are required." });
  }

  try {
    // Fetch current top 10 scores
    const topScores = await HighScore.find().sort({ score: -1 }).limit(10);

    let qualifies = false;
    if (topScores.length < 10) {
      // If less than 10 scores exist, any new score qualifies
      qualifies = true;
    } else {
      // If 10 scores exist, check if the new score is higher than the lowest
      const lowestTopScore = topScores[9].score;
      if (score > lowestTopScore) {
        qualifies = true;
      }
    }

    if (qualifies) {
      // Save the new score
      const newScore = new HighScore({ playerName, score });
      await newScore.save();
      console.log(`[${new Date().toISOString()}] New high score saved:`, newScore);

      // Fetch the updated top 10 list
      const updatedHighScores = await HighScore.find().sort({ score: -1 }).limit(10);
      res.json({ message: "High score added successfully", highScores: updatedHighScores });
    } else {
      // Score doesn't qualify
      console.log(`[${new Date().toISOString()}] Score ${score} by ${playerName} not high enough to enter top 10.`);
      res.json({ message: "Score not high enough to enter top 10", highScores: topScores });
    }

  } catch (err) {
    console.error(`[${new Date().toISOString()}] Error processing high score:`, err);
    res.status(500).json({ error: "Failed to process high score" });
  }
});

// Global error handling
process.on("uncaughtException", (err) => {
  console.error("Uncaught Exception:", err);
});

process.on("unhandledRejection", (reason, promise) => {
  console.error("Unhandled Rejection at:", promise, "reason:", reason);
});

// Start the server
app.listen(PORT, () => {
  console.log(`Server is running on http://localhost:${PORT}`);
});