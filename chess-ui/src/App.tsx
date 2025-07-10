import SetFEN from "./components/setFEN";
import Board from "./components/board";
import EngineOutput from "./components/engineOutput";

function App() {
    return (
        <>
            <h1 className="font-bold text-red-500">Chess</h1>
            <SetFEN />
			<Board />
			<EngineOutput />
        </>
    );
}

export default App;
