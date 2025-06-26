using csWebFrame;

namespace csWebFrame.app
{
    // Session variables for memory game
    public class GameBoard : SessionVar<string[]>
    {
        public GameBoard(UserSession session) : base(session, new string[16])
        {
        }
    }

    public class FlippedCards : SessionVar<List<int>>
    {
        public FlippedCards(UserSession session) : base(session, new List<int>())
        {
        }
    }

    public class MatchedCards : SessionVar<List<int>>
    {
        public MatchedCards(UserSession session) : base(session, new List<int>())
        {
        }
    }

    public class Score : SessionVar<int>
    {
        public Score(UserSession session) : base(session, 0)
        {
        }
    }

    public class Moves : SessionVar<int>
    {
        public Moves(UserSession session) : base(session, 0)
        {
        }
    }

    public class GameMessage : SessionVar<string>
    {
        public GameMessage(UserSession session) : base(session, "")
        {
        }
    }

    public class IsGameComplete : SessionVar<bool>
    {
        public IsGameComplete(UserSession session) : base(session, false)
        {
        }
    }

    // Memory Card Component
    public class MemoryCard : DefaultHtmlComponent
    {
        private int cardIndex;
        private string cardValue;
        private bool isFlipped;
        private bool isMatched;

        public MemoryCard(int index, string value, bool flipped, bool matched)
        {
            cardIndex = index;
            cardValue = value;
            isFlipped = flipped;
            isMatched = matched;
            Name = $"card_{index}";
        }

        public override Dictionary<string, object> GetVariables(UserSession session)
        {
            return new Dictionary<string, object>
            {
                ["cardIndex"] = cardIndex,
                ["cardValue"] = isFlipped || isMatched ? cardValue : "?",
                ["cssClass"] = GetCssClass(),
                ["disabled"] = (isMatched || isFlipped) ? "disabled" : ""
            };
        }

        private string GetCssClass()
        {
            var classes = new List<string> { "memory-card" };
            if (isFlipped) classes.Add("flipped");
            if (isMatched) classes.Add("matched");
            return string.Join(" ", classes);
        }

        public override string GetHtml(UserSession session, PostUrl postUrl)
        {
            var variables = GetVariables(session);
            return $@"<button class=""{variables["cssClass"]}"" 
                             onclick=""flipCard({variables["cardIndex"]})"" 
                             {variables["disabled"]}>
                        {variables["cardValue"]}
                      </button>";
        }
    }

    // Game Board Component
    public class MemoryGameBoard : DefaultHtmlComponent
    {
        private GameBoard gameBoard;
        private FlippedCards flippedCards;
        private MatchedCards matchedCards;

        public MemoryGameBoard(GameBoard board, FlippedCards flipped, MatchedCards matched)
        {
            gameBoard = board;
            flippedCards = flipped;
            matchedCards = matched;
            Name = "GameBoard";
        }

        public override Dictionary<string, object> GetVariables(UserSession session)
        {
            var cards = new Dictionary<string, object>();
            var board = gameBoard.Get();
            var flipped = flippedCards.Get();
            var matched = matchedCards.Get();

            for (int i = 0; i < board.Length; i++)
            {
                var card = new MemoryCard(i, board[i], flipped.Contains(i), matched.Contains(i));
                cards[$"card_{i}"] = card;
            }

            return cards;
        }

        public override string GetHtml(UserSession session, PostUrl postUrl)
        {
            var variables = GetVariables(session);
            var html = "<div class=\"game-board\">";
            
            for (int i = 0; i < 16; i++)
            {
                var card = (MemoryCard)variables[$"card_{i}"];
                postUrl.NewComponent($"card_{i}");
                html += card.GetHtml(session, postUrl);
            }
            
            html += "</div>";
            return html;
        }
    }

    // Card flip button for each card
    public class CardFlipButton : Button
    {
        private int cardIndex;
        private GameBoard gameBoard;
        private FlippedCards flippedCards;
        private MatchedCards matchedCards;
        private Score score;
        private Moves moves;
        private GameMessage gameMessage;
        private IsGameComplete isGameComplete;

        public CardFlipButton(int index, GameBoard board, FlippedCards flipped, MatchedCards matched, 
                             Score gameScore, Moves gameMoves, GameMessage message, IsGameComplete complete)
        {
            cardIndex = index;
            gameBoard = board;
            flippedCards = flipped;
            matchedCards = matched;
            score = gameScore;
            moves = gameMoves;
            gameMessage = message;
            isGameComplete = complete;
            Name = $"FlipCard_{index}";
        }

        public override void OnClick(Dictionary<string, string> data)
        {
            var flipped = flippedCards.Get();
            var matched = matchedCards.Get();

            // Don't flip if card is already flipped or matched
            if (flipped.Contains(cardIndex) || matched.Contains(cardIndex))
                return;

            // Don't allow more than 2 cards to be flipped at once
            if (flipped.Count >= 2)
                return;

            // Add card to flipped list
            flipped.Add(cardIndex);
            flippedCards.Set(flipped);

            // If this is the second card flipped
            if (flipped.Count == 2)
            {
                moves.Set(moves.Get() + 1);
                var board = gameBoard.Get();
                
                // Check if cards match
                if (board[flipped[0]] == board[flipped[1]])
                {
                    // Cards match - add to matched list
                    var matchedList = matchedCards.Get();
                    matchedList.Add(flipped[0]);
                    matchedList.Add(flipped[1]);
                    matchedCards.Set(matchedList);
                    
                    score.Set(score.Get() + 1);
                    gameMessage.Set("Great! You found a pair!");
                    
                    // Check if game is complete
                    if (matchedList.Count == 16)
                    {
                        isGameComplete.Set(true);
                        gameMessage.Set($"Congratulations! You won in {moves.Get()} moves!");
                    }
                    
                    // Clear flipped cards immediately for matches
                    flippedCards.Set(new List<int>());
                }
                else
                {
                    gameMessage.Set("Cards don't match. They will flip back after a moment.");
                    // Don't clear flipped cards yet - let the HTML/JS handle the delay
                }
            }
            else
            {
                gameMessage.Set("Pick another card!");
            }
        }
    }

    // Add a new button to flip cards back when they don't match
    public class FlipBackButton : Button
    {
        private FlippedCards flippedCards;
        private GameMessage gameMessage;

        public FlipBackButton(FlippedCards flipped, GameMessage message)
        {
            Name = "FlipBack";
            flippedCards = flipped;
            gameMessage = message;
        }

        public override void OnClick(Dictionary<string, string> data)
        {
            // Clear the flipped cards (flip them back)
            flippedCards.Set(new List<int>());
            gameMessage.Set("Cards flipped back. Continue playing!");
        }
    }

    // New Game Button
    public class NewGameButton : Button
    {
        private GameBoard gameBoard;
        private FlippedCards flippedCards;
        private MatchedCards matchedCards;
        private Score score;
        private Moves moves;
        private GameMessage gameMessage;
        private IsGameComplete isGameComplete;

        public NewGameButton(GameBoard board, FlippedCards flipped, MatchedCards matched, 
                           Score gameScore, Moves gameMoves, GameMessage message, IsGameComplete complete)
        {
            Name = "New Game";
            gameBoard = board;
            flippedCards = flipped;
            matchedCards = matched;
            score = gameScore;
            moves = gameMoves;
            gameMessage = message;
            isGameComplete = complete;
        }

        public override void OnClick(Dictionary<string, string> data)
        {
            // Create new shuffled deck
            var symbols = new[] { "🎮", "🎯", "🎲", "🎪", "🎨", "🎭", "🎸", "🎺" };
            var deck = new List<string>();
            
            // Add each symbol twice
            foreach (var symbol in symbols)
            {
                deck.Add(symbol);
                deck.Add(symbol);
            }
            
            // Shuffle the deck
            var random = new Random();
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }
            
            // Reset game state
            gameBoard.Set(deck.ToArray());
            flippedCards.Set(new List<int>());
            matchedCards.Set(new List<int>());
            moves.Set(0);
            isGameComplete.Set(false);
            gameMessage.Set("New game started! Find all the matching pairs!");
        }
    }

    // Reset Stats Button
    public class ResetStatsButton : Button
    {
        private Score score;
        private Moves moves;
        private GameMessage gameMessage;

        public ResetStatsButton(Score gameScore, Moves gameMoves, GameMessage message)
        {
            Name = "Reset Stats";
            score = gameScore;
            moves = gameMoves;
            gameMessage = message;
        }

        public override void OnClick(Dictionary<string, string> data)
        {
            score.Set(0);
            moves.Set(0);
            gameMessage.Set("Stats reset!");
        }
    }

    // The main class MUST match the filename for the framework to find it
    public class Memorygame : DefaultPage
    {
        private GameBoard gameBoard;
        private FlippedCards flippedCards;
        private MatchedCards matchedCards;
        private Score score;
        private Moves moves;
        private GameMessage gameMessage;
        private IsGameComplete isGameComplete;
        private MemoryGameBoard gameBoardComponent;
        private NewGameButton newGameButton;
        private ResetStatsButton resetStatsButton;
        private FlipBackButton flipBackButton;
        private List<CardFlipButton> cardButtons;

        public Memorygame(UserSession session) : base(session)
        {
            gameBoard = new GameBoard(session);
            flippedCards = new FlippedCards(session);
            matchedCards = new MatchedCards(session);
            score = new Score(session);
            moves = new Moves(session);
            gameMessage = new GameMessage(session);
            isGameComplete = new IsGameComplete(session);

            // Initialize game if not already started
            if (gameBoard.Get().All(card => string.IsNullOrEmpty(card)))
            {
                InitializeNewGame();
            }

            // Create components
            gameBoardComponent = new MemoryGameBoard(gameBoard, flippedCards, matchedCards);
            newGameButton = new NewGameButton(gameBoard, flippedCards, matchedCards, score, moves, gameMessage, isGameComplete);
            resetStatsButton = new ResetStatsButton(score, moves, gameMessage);
            flipBackButton = new FlipBackButton(flippedCards, gameMessage);

            // Create card flip buttons
            cardButtons = new List<CardFlipButton>();
            for (int i = 0; i < 16; i++)
            {
                cardButtons.Add(new CardFlipButton(i, gameBoard, flippedCards, matchedCards, score, moves, gameMessage, isGameComplete));
            }
        }

        private void InitializeNewGame()
        {
            var symbols = new[] { "🎮", "🎯", "🎲", "🎪", "🎨", "🎭", "🎸", "🎺" };
            var deck = new List<string>();
            
            foreach (var symbol in symbols)
            {
                deck.Add(symbol);
                deck.Add(symbol);
            }
            
            var random = new Random();
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }
            
            gameBoard.Set(deck.ToArray());
            gameMessage.Set("Welcome to Memory Game! Find all the matching pairs!");
        }

        public override Dictionary<string, object> Render()
        {
            var flippedCount = flippedCards.Get().Count;
            var result = new Dictionary<string, object>
            {
                ["title"] = "Memory Game",
                ["score"] = score.Get().ToString(),
                ["moves"] = moves.Get().ToString(),
                ["gameMessage"] = gameMessage.Get(),
                ["isComplete"] = isGameComplete.Get() ? "true" : "false",
                ["flippedCount"] = flippedCount.ToString(),
                ["showFlipBack"] = (flippedCount == 2) ? "true" : "false",
                ["gameBoard"] = gameBoardComponent,
                ["newGameButton"] = newGameButton,
                ["resetStatsButton"] = resetStatsButton,
                ["flipBackButton"] = flipBackButton
            };

            // Add individual card buttons for POST handling
            for (int i = 0; i < cardButtons.Count; i++)
            {
                result[$"cardButton_{i}"] = cardButtons[i];
            }

            return result;
        }
    }
}