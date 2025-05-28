using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using NIMSAP_Lib;

namespace Codename_NIMSAP.Managers;

public static class InputManager
{
    private static MouseState mouse;
    private static KeyboardState keyboard;
    private static byte[] lastData;
    private static Vector2 motion;
    private static Vector2 lastMotion = Vector2.Zero;
    
    // TODO: Переписать это вообще, чтобы отдельные части управления были отдельными функциями (пока что только управление WASD)
    public static byte[]? Update()
    { 
        motion = Vector2.Zero;
        InputType type;
        byte[] data = new byte[2] {128, 128};
        
        // Получение состояний клавиатуры и мыши
        keyboard = Keyboard.GetState();
        mouse = Mouse.GetState();
        // Действия с клавиатуры
        if (keyboard.GetPressedKeyCount() > 0)
        {
            // Выход из игры
            if (keyboard.IsKeyDown(Keys.Escape)) data[0] = (byte)InputType.Exit;
            
            // Движение игрока
            if (keyboard.IsKeyDown(Keys.Up) || keyboard.IsKeyDown(Keys.W))    // Y -1
            {
                motion.Y -= 1;
            }      
            if (keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A))  // X -1
            {
                motion.X -= 1;
            }    
            if (keyboard.IsKeyDown(Keys.Down) || keyboard.IsKeyDown(Keys.S))  // Y +1
            {
                motion.Y += 1;
            }    
            if (keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D)) // X +1
            {
                motion.X += 1;
            }
            // Обработка направления движения
            // Какой ужас
            /*if (motion.X == 0)
            {
                if (motion.Y != 0)
                {
                    if (motion.Y > 0) data[0] = (byte)InputType.MotionUp;
                    else data[0] = (byte)InputType.MotionUp;
                }
                else data[0] = (byte)InputType.MotionStop;
            }
            else if (motion.X < 0)
            {
                if (motion.Y != 0)
                {
                    if (motion.Y > 0) data[0] = (byte)InputType.MotionUpLeft;
                    else data[0] = (byte)InputType.MotionDownLeft;
                }
                else data[0] = (byte)InputType.MotionLeft;
            }
            else
            {
                if (motion.Y != 0)
                {
                    if (motion.Y > 0) data[0] = (byte)InputType.MotionUpRight;
                    else data[0] = (byte)InputType.MotionDownRight;
                }
                else data[0] = (byte)InputType.MotionRight;
            }*/

            // TODO: Добавить остальные клавиши здесь
        }
        // TODO: Сделать обработку действий с мыши )))
        // Действия с мыши
        else
        {
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                
            }
            else if (mouse.MiddleButton == ButtonState.Pressed)
            {
                
            }
            else if (mouse.RightButton == ButtonState.Pressed)
            {
                
            }
        }

        data[0] += (byte)motion.X;
        data[1] += (byte)motion.Y;
        // Проверка на идентичность последнего отправленного и нового действия
        // TODO: Переписать для использования помимо ради ходьбы
        byte[]? result = data;
        if (motion == lastMotion)
        {
            result = null;
        }
        else
        {
            Console.WriteLine($"User Input: {motion}");
            lastMotion = motion;
        }
        // byte[]? result = data.Equals(lastData) ? null : data;
        // lastData = data;

        return result;
    }
}