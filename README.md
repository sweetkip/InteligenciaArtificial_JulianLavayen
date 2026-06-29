InteligenciaArtificial_JulianLavayen

Alumno: Lavayén Julián

Nombre del juego: Wild Watch
Género: Sigilo, Monster tamer
Objetivo: Explorar los distintos hábitats de los Pokémon y capturarlos. Pero ten cuidado, algunos de ellos son extremandamente asustadizos o agresivos. La hierba alta será tu mejor amiga, te servirá para ocultarte de las criaturas mientras las intentas capturar.

Controles:
Movimiento: WASD
Cámara: Mouse
Lanzar PokéBall: Clic izquierdo del mouse
Agacharse: Q
Saltar: Espacio
Correr: Shift izquierdo


Para esta primer entrega, hay tres Pokémon, cada uno con una actitud diferente.
1. El primero simplemente se te acerca si te ve, es amigable.
2. El segundo es temeroso, al verte se tira al agua. Reaparece en su posición inicial a los pocos segundos.
3. El tercero se acerca para pegarte. Si te golpea tres veces, se reinicia el nivel.



SEGUNDA ENTREGA
¡Buenas! Para esta segunda entrega lo que agregué/cambié fue:

Mapa extendido: para agregar las nuevas IAs agrandé el mapa y lo reorganicé.
Decoración.
Shaders: aproveché el proyecto para la materia de programación gráfica, por lo que ahora tiene diversos shaders.
Música: agregué dos músicas y cada Pokémon emite sonidos en sus zonas.
A* y Theta*: lo más relevante, añadí dos Pokémon o IAs para demostrar estos sistemas de pathfindig.
Flocking: por último, añadí un Pokémon pez que se traslada como un banco por el agua.



Ahora sí, me extiendo más sobre las IAs. Les pongo primero el nombre del Pokémon, pero para no ir anotando "el que aparece en tal lugar" en cada uno jaja. Igualmente, están en orden de aparición.

MUDKIP
	IA de la primer entrega.
	Tiene el comportamiento más básico de todos. Está caminando por su zona y si visualiza al jugador se acerca a él. En su zona hay pasto alto, y si el jugador se agacha en él lo deja de ver y perseguir.
	Mudkip tiene la Personality en "Aggresive". Esta crea un nodo de decisiones en el PKMNDecisionTree que tiene 3 posibles Node_Action, cada una llamando a funciones dentro del SteeringBehaviors:
	. Wander: se ejecuta en caso de no ver al jugador. Elige un ángulo aleatorio, se lo multiplica por la rotación actual, se normaliza y se le pasa a la IA para que comience a caminar en esa dirección. Para que no esté constantemente rotando y caminando, dentro del PKMNController tiene un timer, cuando llega a 0 se ejecuta la acción, tras la cual se reinicia el timer.
	. Pursue: se ejecuta en caso de ver al jugador. Intenta predecir la posición futura del target y se dirige allí.



WIMPOD
	IA de la primer entrega.
	Está sobre un hongo haciendo una vigía, cada n segundos rota 90° sobre el eje Y en el lugar. Si ve al jugador, se lanza al agua para respawnear en el mismo lugar a los n segundos. En su zona hay pasto alto para que el jugador se oculte y lo intente capturar desde ahí.
	Wimpod lleva la Personality de su nombre, la cual crea su propio nodo de decisiones dentro del PKMNDecisionTree. Este puede efectuar una de dos acciones:
	. Tower: al no tener al jugador en su vista, se efectúa esta acción. Cambia el state a "W_Tower", el cual llama a una función dentro del PKMNController que se encarga de rotar el eje Y del Pokémon en 90° cada n segundos. En este estado está constantemente revisando si en su área de visión localiza al player. En caso de hacerlo, llama a...
	. ToLake: cambia el state a "W_Lake", llamando así a la función Seek de los SteeringBehaviours, fijando como target un GameObject dentro del agua. Este hace que Wimpod se diriga a dicho GameObject. Una vez colisiona con el objeto con tag "Water" se desactiva su render y comienza una corrutina, haciendo que a los n segundos reaparezca en su punto inicial con el render activado y el state "W_Tower".



SANDYGAST
	IA de la segunda entrega.
	Sandygast está en la arena esperando a ver al jugador. En cuanto lo vea se mete bajo la arena, elige un nodo al azar y se desplaza debajo de la arena hasta llegar a él. Una vez en su nuevo nodo vuelve a vigilar.
	Dentro del PKMNController tiene la Personality "Sandygast", la cual utiliza dos estados: "S_Idle" y "S_Moving". Dicha personalidad crea un nodo de decisiones en el PKMNDecisionTree con dos posibles acciones:
	. StayIdle: se elige cuando no está viendo al jugador o recién llegó a un nuevo nodo. Tiene su propio idle porque se encarga de que su mesh se encuentre sobre la tierra tras el viaje. Se queda quieto esperando a ver al jugador.
	. TriggerDig: en caso de localizar al player se activa. Acá se realizan dos cosas. Primero, se llama a una función dentro del PKMNController que efectúa el código de A*. Esta elige un nodo aleatorio dentro del sistema y evalúa cual es la ruta más eficiente para llegar a la meta desde su nodo actual. También se encarga de sumergir la mesh de Sandygast en la arena. Una vez la ruta está fijada, se cambia el estado a "S_Moving", el cual ahora sí invoca a los SteeringBehavior. Llama a la función FollowPath que se encarga de mover al Pokémon por los nodos recorriendo el camino. Calcula la dirección entre el nodo actual y el siguiente, se normaliza y se dirige allí.



GIMMIGHOUL
	IA de la segunda entrega.
	A este Pokémon no le importa el jugador, solo las monedas. Las busca constantemente, cada que recolecta una aparece otra en uno de los nodos de manera aleatoria y la persigue.
	El PKMNController le asigna la Personality "Gimmighoul", la cual aprovecha los State "G_Search" y "G_Move". La lógica que sigue, al utilizar pathfinding, es muy similar a la de Sandygast. Su personalidad en el PKMNDecisionTree crea un nodo de decisiones con dos posibles acciones:
	. Search: primer comportamiento de la IA. En caso de no tener la moneda se activa. Cambia el state a "G_Search", el cual funciona muy parecido al triggerDig de Sandygast. Este llama a una función dentro de PKMNController que efectúa el código de Theta*, buscando en sus nodos uno ocupado por un objeto con el tag "GimmighoulCoin". Fija dicho nodo como objetivo y traza la ruta más óptima. Fija la ruta, cambia el state a "G_Move".
	. Move: ya teniendo el camino a seguir, se cambia al "G_Move". El cambio de estado llama, dentro del PKMNController, a la función FollowPaht de los SteeringBehaviours. Esta, nuevamente, se encarga de mover al Pokémon por los nodos recorriendo el camino. Calcula la dirección entre el nodo actual y el siguiente, se normaliza y se dirige allí. Alcanzada la moneda se cambia el state a "G_Search" para volver a comenzar.



TINKATON
	IA de la primer entrega.
	Pokémon agresivo, una vez el jugador entra en su campo visual se acerca a atacarlo. Es el único de todos que le puede bajar la vida al jugador. Al igual que los primeros dos, en su zona hay pasto alto para ocultarse.
	Tinkaton tiene la Personality "Attack", la cual genera un decision node dentro del PKMNDecisionTree con las siguientes posibilidades:
	. Wander: se ejecuta en caso de no ver al jugador. Cambia el state a "Wander", el cual dentro del PKMNController llama a la función Wander de los SteeringBehaviours. Elige un ángulo aleatorio, se lo multiplica por la rotación actual, se normaliza y se le pasa a la IA para que comience a caminar en esa dirección. Para que no esté constantemente rotando y caminando, dentro del PKMNController tiene un timer, cuando llega a 0 se ejecuta la acción, tras la cual se reinicia el timer.
	. Pursue: en caso de ver al jugador dentro de su área de visión, cambia el state a "Pursue". Dicho estado llama a la función del mismo nombre dentro de los SteeringBehaviours. Intenta predecir la posición futura del target y se dirige allí.
	. Attack: una vez visto el jugador, en caso de estar dentro su área de alcance, cambia al estado "Attack" y llama a la función Attack, valga la redundancia. Esta genera una instancia del martillo de Tinkaton y gira en un área. Si colisionó con el jugador, se le resta 1 punto de vida.




WISHIWASHI
	IA de la segunda entrega.
	Wishiwashi no es capturable por el jugador ni tiene una interacción especial con él. Solo está recorriendo las aguas con su cardumen.
	Utiliza la lógica de Flocking. En la escena hay un FlockManager que se encarga de su generación, instanciando en un área dada n cantidad de Wishiwashis, de mandarle a cada instancia sus características y de controlar su centro, el cual nos ayuda más adelante. Cada uno de estos Pokémon tiene el componente FlockAgent, el cual en base a los valores dados del FlockManager se dedica a darles su dirección. Esta dirección se calcula teniendo en cuenta:
	. Separation: si dos Wishiwashis están en un radio cercano, se repelen.
	. Aligment: calcula un promedio de las velocidades lineales de las demás instancias dentro de un radio, para tratar de imitarlos y que parezcan un grupo.
	. Cohesion: calcula el centro de masa del grupo de instancias dentro de un radio y traza un vector desde ese punto hasta el centro del cardumen que maneja el FlockManager.
	. TargetForce: genera un vector entre el transform propio y el siguiente objetivo del cardumen.
	. BoundsForce: en caso de que un Wishiwashi se aleje demasiado del cardumen, se traza un vector hacia el centro del cardumen para regresar.

	Teniendo todos estos vectores individuales, se calcula la aceleración para sumársela a la velocidad de cada Wishishashi. En caso de ir demasiado lento, la velocidad se reemplaza por la minSpeed, y lo mismo en caso de ir demasiado rápido con la maxSpeed.
