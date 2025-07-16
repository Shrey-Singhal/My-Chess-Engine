import ChessGame from "./components/ChessGame";
import EngineOutput from "./components/engineOutput";

function App() {
    return (
        <>
            <h1 className="font-bold text-red-500">Chess</h1>
            <ChessGame />
			<EngineOutput />
        </>
    );
}

export default App;
