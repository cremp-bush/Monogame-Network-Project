namespace NIMSAP_Lib;

/* TODO: РАСШИРИТЬ ВИДЫ ДЕЙСТВИЙ КЛИЕНТА */
/* Типы движения */
public enum InputType
{
    Exit,       /* Выход из игры */
    /* Движение персоанажа */
    MotionUpLeft,   MotionUp,   MotionUpRight,
    MotionLeft,     MotionStop, MotionRight,
    MotionDownLeft, MotionDown, MotionDownRight,
}